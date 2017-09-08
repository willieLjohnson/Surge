using UnityEngine;

public class TimeManager : MonoBehaviour
{

    public float slowdownFactor = 0.0f;
    public float slowdownLength = .01f;

    void Update()
    {
		Time.timeScale += (1f / slowdownLength) * Time.unscaledDeltaTime;
		Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
		Time.fixedDeltaTime = Time.timeScale * .02f;
    }

    public void DoSlowMotion()
    {
        Time.timeScale = slowdownFactor;
		Time.fixedDeltaTime = Time.timeScale * .02f;
    }
}
