using UnityEngine;

public class ChargeHazard : MonoBehaviour
{
    public float m_Speed = 3;

    void Start()
    {
        
    }

    void Update()
    {
        transform.Translate(Vector3.forward * m_Speed * Time.deltaTime, Space.World);
    }
}
