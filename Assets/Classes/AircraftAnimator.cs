using UnityEngine;

public class AircraftAnimator : MonoBehaviour
{
    protected class RotationAnimationData
    {
        public void Initialize(Animator _animator, string variableName, string clipName, string layerName)
        {
            animator = _animator;
            variableID = Animator.StringToHash(variableName);
            animator.Play(clipName, animator.GetLayerIndex(layerName), time = 0.5f);
            status = 0;
            speed = 1.0f;
            middleTime = 0.5f;
        }

        public void Update(int state) { status = state; Update(); }
        public void Update()
        {
            switch (status)
            {
                case -1:
                    time = Mathf.Clamp01(time - Time.deltaTime * speed);
                    break;
                case 0:
                    {
                        float gap = time - middleTime;
                        if (Mathf.Abs(gap) < 0.05f)
                            time = middleTime;
                        else
                            time -= Time.deltaTime * speed * Mathf.Sign(gap);
                    }
                    break;
                case 1:
                    time = Mathf.Clamp01(time + Time.deltaTime * speed);
                    break;
                default:
                    status = 0;
                    break;
            }

            animator.SetFloat(variableID, time);
        }
        public void SetSpeed(float value) { speed = value; }
        public void SetMiddleTime(float value) { middleTime = value; }
        public void SetMotionTime(float value) { animator.SetFloat(variableID, Mathf.Clamp01(time)); }

        public int status;
        private int variableID;
        private float time;
        private float speed;
        private float middleTime;
        private Animator animator;

        public void DebugMotionTime()
        {
            Debug.Log(animator.GetFloat(variableID));
            //Debug.Log(animator.GetCurrentAnimatorStateInfo(layerID).normalizedTime.ToString());
        }
    }

    protected class PartsAnimationData
    {
        public void Initialize(Animator _animator, string variableName, string clipName, string layerName, int _baseStatus)
        {
            state = baseState = _baseStatus;
            animator = _animator;
            variableID = Animator.StringToHash(variableName);
            animator.Play(clipName, animator.GetLayerIndex(layerName), time = (baseState == -1 ? 0 : 1));
            speed = 1.0f;
        }

        public void Update(int state)
        {
            this.state = state;
            Update();
        }
        public void Update()
        {
            switch (state)
            {
                case -1:
                    time = Mathf.Clamp01(time - Time.deltaTime * speed);
                    break;
                case 0:
                    switch (baseState)
                    {
                        case -1:
                            time = Mathf.Clamp01(time - Time.deltaTime * speed);
                            break;
                        case 1:
                            time = Mathf.Clamp01(time + Time.deltaTime * speed);
                            break;
                        default:
                            break;
                    }
                    break;
                case 1:
                    time = Mathf.Clamp01(time + Time.deltaTime * speed);
                    break;
                default:
                    state = 0;
                    break;
            }

            animator.SetFloat(variableID, time);
        }
        public void SetSpeed(float value) { speed = value; }
        public void SetMotionTime(float value) { animator.SetFloat(variableID, Mathf.Clamp01(time)); }

        public int state;
        private int baseState;
        private int variableID;
        private float time;
        private float speed;
        private Animator animator;
    }


    protected PartsAnimationData AddAnimationData(string variableName, string clipName, string layerName, int baseState, bool isBody = true)
    {
        PartsAnimationData newInstance = new PartsAnimationData();
        newInstance.Initialize((isBody ? bodyAnimator : gearAnimator), variableName, clipName, layerName, baseState);
        return newInstance;
    }
    protected RotationAnimationData AddAnimationData(string variableName, string clipName, string layerName, bool isBody = true)
    {
        RotationAnimationData newInstance = new RotationAnimationData();
        newInstance.Initialize((isBody ? bodyAnimator : gearAnimator), variableName, clipName, layerName);
        return newInstance;
    }

    [SerializeField] public Control control = null;
    [SerializeField] protected Animator bodyAnimator = null;
    [SerializeField] protected Animator gearAnimator = null;
}
