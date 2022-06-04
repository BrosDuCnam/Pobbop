using UnityEngine;

public class TutorialTarget : Target
{
    private void OnCollisionEnter(Collision col)
    {
        if (!enabled) return;
        if (col.gameObject.CompareTag("Ball"))
        {
            StoryManager.Instance.targets.Remove(this);
            Destroy(this);
        }
    }
}