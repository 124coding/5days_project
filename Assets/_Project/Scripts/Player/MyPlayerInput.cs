using UnityEngine;

public class MyPlayerInput : MonoBehaviour
{
    public float H { get; private set; }
    public float V { get; private set; }
    public bool IsAttack { get; private set; }
    public bool IsRoll { get; private set; }

    void Update()
    {
        // 입력 관리
        H = Input.GetAxisRaw("Horizontal");
        V = Input.GetAxisRaw("Vertical");
        IsAttack = Input.GetMouseButton(0);
        IsRoll = Input.GetKeyDown(KeyCode.Space);
    }
}
