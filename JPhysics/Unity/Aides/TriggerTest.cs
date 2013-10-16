namespace JPhysics.Unity.Aides
{
    using UnityEngine;

    class TriggerTest : MonoBehaviour
    {
        JCollider col;

        void Awake()
        {
            col = GetComponent<JCollider>();
            col.TriggerEnter += EnterToTrigger;
            col.TriggerStay += StayInTrigger;
            col.TriggerExit += ExitFromTrigger;
        }
        
        void EnterToTrigger(TriggerInfo i)
        {
            Debug.Log("Enter to trigger " + i.Body.name);
        }

        void StayInTrigger(TriggerInfo i)
        {
            //Debug.Log("Stay in trigger " + i.Body.name);
        }

        void ExitFromTrigger(TriggerInfo i)
        {
            Debug.Log("Exit from trigger " + i.Body.name);
        }
    }
}
