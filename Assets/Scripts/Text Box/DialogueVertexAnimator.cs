using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class DialogueVertexAnimator {

    public bool textAnimating = false;
    private bool stopAnimating = false;

    private TMP_Text textBox;
    private float textAnimationScale;
    
    private SoundManager _soundManager;
    private MusicManager _musicManager;

    private Shake _camera;

    public DialogueManager _parent;
    private Flash _flash;
    public int visableCharacterIndex;
    public int actualCharacterIndex;
    public int charCount;
    private Color32[][] originalColors;
    private string _processedMessage;
    
    float secondsPerCharacter = 2f / 60f;
    private List<DialogueCommand> _commands;
    public DialogueVertexAnimator(TMP_Text _textBox) {
        if (_textBox != null)
        {
            textBox = _textBox;
            textAnimationScale = textBox.fontSize;
        }
        
        _soundManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<SoundManager>();
        _musicManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<MusicManager>();
        _camera = GameObject.FindWithTag("MainCamera").GetComponent<Shake>();
        _flash = GameObject.FindObjectOfType<Flash>();
    }

    private static readonly Color32 clear = new Color32(0, 0, 0, 0);
    private const float CHAR_ANIM_TIME = 0.07f;
    private static readonly Vector3 vecZero = Vector3.zero;
    
    public IEnumerator AnimateTextIn(List<DialogueCommand> commands, string processedMessage, string voice_sound, Action onFinish, int startingIndex=0, Vector2? pitchRange=null, float blipFrequency=4f/60f) {
        textAnimating = true;
        _commands = commands;
        float timeOfLastCharacter = Time.unscaledTime - secondsPerCharacter;

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

        
        processedMessage += " ";
        textBox.text = processedMessage;
        _processedMessage = processedMessage;
        processedMessage = Globals.RemoveRichText(processedMessage);
        textBox.enableAutoSizing = false;
        textBox.ForceMeshUpdate();

        while (textBox.textInfo.lineCount > _parent._tempBox.MaxLines)
        {
            int lineBreakCount = textBox.text.Length - textBox.text.Replace("\n", string.Empty).Length;
            
            if (lineBreakCount <= 0) break;

            if (textBox.alignment is TextAlignmentOptions.Center or TextAlignmentOptions.Top) _processedMessage = Globals.ReplaceLastOccurrence(_processedMessage, "\n", " ");
            else _processedMessage = Globals.ReplaceFirstOccurrence(_processedMessage, "\n", " ");
            
            textBox.text = _processedMessage;
            processedMessage = Globals.RemoveRichText(processedMessage);
            textBox.ForceMeshUpdate();
        }
        
        textBox.enableAutoSizing = true;
        textBox.ForceMeshUpdate();

        TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
        UpdateColors(textInfo);
        charCount = textInfo.characterCount;
        float[] charAnimStartTimes = new float[charCount];
        for (int i = 0; i < charCount; i++) {
            charAnimStartTimes[i] = -1; //indicate the character as not yet started animating.
        }
        visableCharacterIndex = startingIndex;
        actualCharacterIndex = visableCharacterIndex;

        textBox.text = _processedMessage.Substring(0, visableCharacterIndex);
        SetColorUntilIndex(visableCharacterIndex);
        textBox.text = _processedMessage.Substring(0, actualCharacterIndex);
        textBox.ForceMeshUpdate();
        yield return null;
        textBox.text = _processedMessage;
        textBox.ForceMeshUpdate();
        textInfo = textBox.textInfo;
        UpdateColors(textInfo);

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
                        PlayDialogueSound(voice_sound, (Vector2) pitchRange, blipFrequency, !Char.IsWhiteSpace(processedMessage[visableCharacterIndex]));
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
            try { 
                textBox.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32); 
                for (int i = 0; i < textInfo.meshInfo.Length; i++) {
                    TMP_MeshInfo theInfo = textInfo.meshInfo[i];
                    theInfo.mesh.vertices = theInfo.vertices;
                    textBox.UpdateGeometry(theInfo.mesh, i);
                }
            } catch {}
            
            yield return null;
        }
    }

    private void UpdateColors(TMP_TextInfo textInfo)
    {
        originalColors = new Color32[textInfo.meshInfo.Length][];
        for (int i = 0; i < originalColors.Length; i++) {
            Color32[] theColors = textInfo.meshInfo[i].colors32;
            originalColors[i] = new Color32[theColors.Length];
            Array.Copy(theColors, originalColors[i], theColors.Length);
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
                        secondsPerCharacter = command.floatValue / 1000f;
                        break;
                    case DialogueCommandType.Sound:
                        _soundManager.Play(command.stringValue);
                        break;
                    case DialogueCommandType.Music:
                        switch (command.stringValue)
                        {
                            case "fadeout":
                                _musicManager.FadeOut(1f);
                                break;
                            case "stop":
                                _musicManager.FadeOut();
                                break;
                            case "continue":
                                _musicManager.FadeIn();
                                break;
                            case "fadein":
                                _musicManager.FadeIn(1f);
                                break;
                            default:
                                _musicManager.Play(command.stringValue);
                                break;
                        }
                        break;
                    case DialogueCommandType.Lowpass:
                        _musicManager.SetLowpass(0.1f, command.floatValue);
                        break;
                    case DialogueCommandType.Shake:
                        _camera.maxShakeDuration = command.floatValue;
                        _camera.multiplier = command.floatValueTwo;
                        _camera.enabled = true;
                        break;
                    case DialogueCommandType.Flash:
                        _flash.StartFlash();
                        break;
                    case DialogueCommandType.Color:
                        string color = "</color>";
                        switch (command.stringValue)
                        {
                            case "white":
                            case "default":
                                color = "<color=#ffffff>";
                                break;
                            case "red":
                                color = "<color=#ff0000>";
                                break;
                            case "green":
                                color = "<color=#00f000>";
                                break;
                            case "blue":
                                color = "<color=#68c0f0>";
                                break;
                        }

                        actualCharacterIndex = 0;
                        for (int j = 0; j < visableCharacterIndex; j++)
                        {
                            if (_processedMessage[actualCharacterIndex] == '<')
                            {
                                while (_processedMessage[actualCharacterIndex] != '>')
                                {
                                    actualCharacterIndex += 1;
                                }

                                actualCharacterIndex += 1;
                            }
                            actualCharacterIndex += 1;
                        }

                        _processedMessage = _processedMessage.Substring(0, actualCharacterIndex) + color +
                                            _processedMessage.Substring(actualCharacterIndex);
                        textBox.text = _processedMessage;
                        textBox.ForceMeshUpdate();
                        UpdateColors(textBox.textInfo);
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
    
    public void QuickEnd(bool loopThroughEvents=true) {
        if (textAnimating) {
            stopAnimating = true;
            textAnimating = false;
            float f = 1000f;
            
            foreach (DialogueCommand command in _commands)
            {
                if (command.position < visableCharacterIndex) continue;
                if (!loopThroughEvents && command.type != DialogueCommandType.Color && command.type != DialogueCommandType.Music && command.type != DialogueCommandType.TextSpeedChange) continue;
                if (command.type != DialogueCommandType.Pause)
                {
                    ExecuteCommandsForCurrentIndex(new List<DialogueCommand>() {command}, command.position, ref secondsPerCharacter, ref f);
                }
            }
        }
    }

    private void SetColorUntilIndex(int index)
    {
        float f = 1000f;
            
        foreach (DialogueCommand command in _commands)
        {
            if (command.position >= index) continue;
            if (command.type != DialogueCommandType.Color) continue;
            ExecuteCommandsForCurrentIndex(new List<DialogueCommand>() {command}, command.position, ref secondsPerCharacter, ref f);
        }
    }

    private float timeUntilNextDialogueSound = 0;
    private float lastDialogueSound = 0;
    private void PlayDialogueSound(String voice_sound, Vector2 pitchRange, float blipFrequency, bool canPlay) {
        if (Time.unscaledTime - lastDialogueSound > timeUntilNextDialogueSound) {
            if (!(_parent.SkipTextButtonHeld && visableCharacterIndex == 0) && canPlay)
            {
                timeUntilNextDialogueSound = blipFrequency;
                lastDialogueSound = Time.unscaledTime;
                
                System.Random rand = new System.Random();

                float pitch = rand.Next((int) pitchRange.x, (int) pitchRange.y) / 100f;
                pitch += 1;
                _soundManager.Play(voice_sound, pitch);
            }
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
