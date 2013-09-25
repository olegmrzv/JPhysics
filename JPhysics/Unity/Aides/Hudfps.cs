namespace JPhysics.Unity.Aides
{
    using UnityEngine;

    public class Hudfps : MonoBehaviour
    {
        public float UpdateInterval = 0.5F;

        private float accum;
        private int frames;
        private float timeleft;

        void Start()
        {
            if (!guiText)
            {
                Debug.Log("UtilityFramesPerSecond needs a GUIText component!");
                enabled = false;
                return;
            }
            timeleft = UpdateInterval;
        }

        void Update()
        {
            timeleft -= Time.deltaTime;
            accum += Time.timeScale / Time.deltaTime;
            ++frames;

            if (!(timeleft <= 0.0)) return;

            var fps = accum / frames;
            var format = System.String.Format("{0:F2} FPS", fps);
            guiText.text = format;

            if (fps < 30)
                guiText.material.color = Color.yellow;
            else
                if (fps < 10)
                    guiText.material.color = Color.red;
                else
                    guiText.material.color = Color.green;
            timeleft = UpdateInterval;
            accum = 0.0F;
            frames = 0;
        }
    }

}
