using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueVertexAnimator {
    public bool textAnimating = false;
    private bool stopAnimating = false;

    private readonly TMP_Text textBox;
    private readonly float textAnimationScale;

    //public DialogueManager _parent;
    
    float secondsPerCharacter = 2f / 60f;
    private List<DialogueCommand> _commands;
    public DialogueVertexAnimator(TMP_Text _textBox) {
        textBox = _textBox;
        textAnimationScale = textBox.fontSize;
    }

    private static readonly Color32 clear = new Color32(0, 0, 0, 0);
    private const float CHAR_ANIM_TIME = 0.07f;
    private static readonly Vector3 vecZero = Vector3.zero;
    public IEnumerator AnimateTextIn(List<DialogueCommand> commands, string processedMessage, string voice_sound, Action onFinish, int startingIndex=0) {
        textAnimating = true;
        _commands = commands;
        float timeOfLastCharacter = 0;

        TextAnimInfo[] textAnimInfo = SeparateOutTextAnimInfo(commands);
        TMP_TextInfo textInfo = textBox.textInfo;
        for (int i = 0; i < textInfo.meshInfo.Length; i++) //Clear the mesh 
        {
            TMP_MeshInfo meshInfer = textInfo.meshInfo[i];
            if (meshInfer.vertices != null) {
                for (int j = 0; j < meshInfer.vertices.Length; j++) {
                    meshInfer.vertices[j] = vecZero;
                }
            }
        }

        textBox.text = processedMessage;
        textBox.ForceMeshUpdate();

        TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
        Color32[][] originalColors = new Color32[textInfo.meshInfo.Length][];
        for (int i = 0; i < originalColors.Length; i++) {
            Color32[] theColors = textInfo.meshInfo[i].colors32;
            originalColors[i] = new Color32[theColors.Length];
            Array.Copy(theColors, originalColors[i], theColors.Length);
        }
        int charCount = textInfo.characterCount;
        float[] charAnimStartTimes = new float[charCount];
        for (int i = 0; i < charCount; i++) {
            charAnimStartTimes[i] = -1; //indicate the character as not yet started animating.
        }
        int visableCharacterIndex = startingIndex;
        while (true) {
            if (stopAnimating) {
                for (int i = visableCharacterIndex; i < charCount; i++) {
                    charAnimStartTimes[i] = Time.unscaledTime;
                }
                visableCharacterIndex = charCount;
                FinishAnimating(onFinish);
            }
            if (ShouldShowNextCharacter(secondsPerCharacter, timeOfLastCharacter)) {
                if (visableCharacterIndex <= charCount) {
                    ExecuteCommandsForCurrentIndex(commands, visableCharacterIndex, ref secondsPerCharacter, ref timeOfLastCharacter);
                    _commands = commands;
                    if (visableCharacterIndex < charCount && ShouldShowNextCharacter(secondsPerCharacter, timeOfLastCharacter)) {
                        charAnimStartTimes[visableCharacterIndex] = Time.unscaledTime;
                        PlayDialogueSound(voice_sound);
                        visableCharacterIndex++;
                        timeOfLastCharacter = Time.unscaledTime;
                        if (visableCharacterIndex == charCount) {
                            FinishAnimating(onFinish);
                        }
                    }
                }
            }
            for (int j = 0; j < charCount; j++) {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[j];
                if (charInfo.isVisible) //Invisible characters have a vertexIndex of 0 because they have no vertices and so they should be ignored to avoid messing up the first character in the string whic also has a vertexIndex of 0
                {
                    int vertexIndex = charInfo.vertexIndex;
                    int materialIndex = charInfo.materialReferenceIndex;
                    Color32[] destinationColors = textInfo.meshInfo[materialIndex].colors32;
                    Color32 theColor = j < visableCharacterIndex ? originalColors[materialIndex][vertexIndex] : clear;
                    destinationColors[vertexIndex + 0] = theColor;
                    destinationColors[vertexIndex + 1] = theColor;
                    destinationColors[vertexIndex + 2] = theColor;
                    destinationColors[vertexIndex + 3] = theColor;

                    Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;
                    Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;
                    float charSize = 1;
                    float charAnimStartTime = charAnimStartTimes[j];
                    // if (charAnimStartTime >= 0) {
                    //     float timeSinceAnimStart = Time.unscaledTime - charAnimStartTime;
                    //     charSize = Mathf.Min(1, timeSinceAnimStart / CHAR_ANIM_TIME);
                    // }

                    Vector3 animPosAdjustment = GetAnimPosAdjustment(textAnimInfo, j, textBox.fontSize, Time.unscaledTime);
                    Vector3 offset = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;
                    destinationVertices[vertexIndex + 0] = ((sourceVertices[vertexIndex + 0] - offset) * charSize) + offset + animPosAdjustment;
                    destinationVertices[vertexIndex + 1] = ((sourceVertices[vertexIndex + 1] - offset) * charSize) + offset + animPosAdjustment;
                    destinationVertices[vertexIndex + 2] = ((sourceVertices[vertexIndex + 2] - offset) * charSize) + offset + animPosAdjustment;
                    destinationVertices[vertexIndex + 3] = ((sourceVertices[vertexIndex + 3] - offset) * charSize) + offset + animPosAdjustment;
                }
            }

            try
            {
                textBox.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    TMP_MeshInfo theInfo = textInfo.meshInfo[i];
                    theInfo.mesh.vertices = theInfo.vertices;
                    textBox.UpdateGeometry(theInfo.mesh, i);
                }
            }
            catch
            {}
            
            yield return null;
        }
    }

    private void ExecuteCommandsForCurrentIndex(List<DialogueCommand> commands, int visableCharacterIndex, ref float secondsPerCharacter, ref float timeOfLastCharacter) {
        for (int i = 0; i < commands.Count; i++) {
            DialogueCommand command = commands[i];
            if (command.position == visableCharacterIndex) {
                switch (command.type) {
                    case DialogueCommandType.Pause:
                        timeOfLastCharacter = Time.unscaledTime + command.floatValue;
                        break;
                    case DialogueCommandType.TextSpeedChange:
                        secondsPerCharacter = command.floatValue / 60f;
                        break;
                    case DialogueCommandType.Sound:
                        Globals.SoundManager.Play(command.stringValue);
                        break;
                    case DialogueCommandType.Music:
                        switch (command.stringValue)
                        {
                            case "fadeout":
                                Globals.MusicManager.fadeOut(1.5f);
                                break;
                            case "stop":
                                Globals.MusicManager.fadeOut();
                                break;
                            case "continue":
                                Globals.MusicManager.fadeIn();
                                break;
                            default:
                                Globals.MusicManager.Play(command.stringValue);
                                break;
                        }
                        break;
                    case DialogueCommandType.Lowpass:
                        Globals.MusicManager.SetLowpass(0.1f, command.floatValue);
                        break;
                    case DialogueCommandType.Shake:
                        Shake[] objects = (Shake[]) GameObject.FindObjectsOfType(typeof(Shake));
                        foreach (Shake obj in objects)
                        {
                            obj.maxShakeDuration = command.floatValue;
                            obj.enabled = true;
                        }
                        break;
                    case DialogueCommandType.Flash:
                        //_parent._tempBox.GetComponent<Animator>().Play("Flash");
                        break;
                }
                commands.RemoveAt(i);
                i--;
            }
        }
    }

    private void FinishAnimating(Action onFinish) {
        textAnimating = false;
        stopAnimating = false;
        onFinish?.Invoke();
    }

    private const float NOISE_MAGNITUDE_ADJUSTMENT = 0.15f;
    private const float NOISE_FREQUENCY_ADJUSTMENT = 20f;
    private const float WAVE_MAGNITUDE_ADJUSTMENT = 0.25f;
    private const float WAVE_FREQUENCY_ADJUSTMENT = 0.20f;
    private Vector3 GetAnimPosAdjustment(TextAnimInfo[] textAnimInfo, int charIndex, float fontSize, float time) {
        float x = 0;
        float y = 0;
        for (int i = 0; i < textAnimInfo.Length; i++) {
            TextAnimInfo info = textAnimInfo[i];
            if (charIndex >= info.startIndex && charIndex < info.endIndex) {
                if (info.type == TextAnimationType.shake) {
                    float scaleAdjust = fontSize * NOISE_MAGNITUDE_ADJUSTMENT;
                    x += (Mathf.PerlinNoise((charIndex + time) * NOISE_FREQUENCY_ADJUSTMENT, 0) - 0.5f) * scaleAdjust;
                    y += (Mathf.PerlinNoise((charIndex + time) * NOISE_FREQUENCY_ADJUSTMENT, 1000) - 0.5f) * scaleAdjust;
                } else if (info.type == TextAnimationType.wave) {
                    x += Mathf.Sin((charIndex * 1.5f) * WAVE_FREQUENCY_ADJUSTMENT - (time * 6) - 180) * fontSize * WAVE_MAGNITUDE_ADJUSTMENT;
                    y += Mathf.Sin((charIndex * 1.5f) * WAVE_FREQUENCY_ADJUSTMENT - (time * 6)) * fontSize * WAVE_MAGNITUDE_ADJUSTMENT;
                }
            }
        }
        return new Vector3(x, y, 0);
    }

    private static bool ShouldShowNextCharacter(float secondsPerCharacter, float timeOfLastCharacter) {
        return (Time.unscaledTime - timeOfLastCharacter) > secondsPerCharacter;
    }
    public void QuickEnd() {
        if (textAnimating) {
            stopAnimating = true;
            float f = 1000f;
            foreach (DialogueCommand command in _commands)
            {
                if (command.type != DialogueCommandType.Pause)
                {
                    ExecuteCommandsForCurrentIndex(new List<DialogueCommand>() {command}, command.position, ref secondsPerCharacter, ref f);
                }
            }
        }
    }

    private float timeUntilNextDialogueSound = 0;
    private float lastDialogueSound = 0;
    private void PlayDialogueSound(String voice_sound) {
        if (Time.unscaledTime - lastDialogueSound > timeUntilNextDialogueSound) {
            timeUntilNextDialogueSound = 4/60f;
            lastDialogueSound = Time.unscaledTime;
            Globals.SoundManager.Play(voice_sound);
        }
    }

    private TextAnimInfo[] SeparateOutTextAnimInfo(List<DialogueCommand> commands) {
        List<TextAnimInfo> tempResult = new List<TextAnimInfo>();
        List<DialogueCommand> animStartCommands = new List<DialogueCommand>();
        List<DialogueCommand> animEndCommands = new List<DialogueCommand>();
        for (int i = 0; i < commands.Count; i++) {
            DialogueCommand command = commands[i];
            if (command.type == DialogueCommandType.AnimStart) {
                animStartCommands.Add(command);
                commands.RemoveAt(i);
                i--;
            } else if (command.type == DialogueCommandType.AnimEnd) {
                animEndCommands.Add(command);
                commands.RemoveAt(i);
                i--;
            }
        }
        if (animStartCommands.Count != animEndCommands.Count) {
            Debug.LogError("Unequal number of start and end animation commands. Start Commands: " + animStartCommands.Count + " End Commands: " + animEndCommands.Count);
        } else {
            for (int i = 0; i < animStartCommands.Count; i++) {
                DialogueCommand startCommand = animStartCommands[i];
                DialogueCommand endCommand = animEndCommands[i];
                tempResult.Add(new TextAnimInfo {
                    startIndex = startCommand.position,
                    endIndex = endCommand.position,
                    type = startCommand.textAnimValue
                });
            }
        }
        return tempResult.ToArray();
    }
}

public struct TextAnimInfo {
    public int startIndex;
    public int endIndex;
    public TextAnimationType type;
}
