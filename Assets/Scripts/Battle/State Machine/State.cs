using System.Collections;
using UnityEngine;

namespace Battle.State_Machine
{
    public class State
    {
        protected BattleManager _battleManager;
        
        public State(BattleManager bm)
        {
            _battleManager = bm;
        }

        public virtual IEnumerator EnterState()
        {
            yield break;
        }
        
        public virtual IEnumerator UpdateState()
        {
            yield break;
        }
        
        public virtual IEnumerator ExitState()
        {
            yield break;
        }
        
        public virtual IEnumerator OnClick()
        {
            yield break;
        }
        
        public virtual IEnumerator OnBack()
        {
            yield break;
        }
        
        public virtual void CheckStates() {}
    }
}