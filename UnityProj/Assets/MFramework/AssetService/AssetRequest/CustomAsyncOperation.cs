using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.AssetService
{
	public abstract class CustomAsyncOperation : CustomYieldInstruction
	{
        private bool isDone;
        public event Action<CustomAsyncOperation> Completed;

        public bool IsDone
        {
            get
            {
                return isDone;
            }
            private set
            {
                isDone = value;
            }
        }

        public float Progress
        {
            get
            {
                return OnProgress();
            }
        }

        public override bool keepWaiting
        {
            get
            {
                return !IsDone;
            }
        }

        protected abstract float OnProgress();

        protected void CompletedInvoke()
        {
            IsDone = true;
            if (Completed != null)
            {
                Completed.Invoke(this);
            }
        }
    }
}
