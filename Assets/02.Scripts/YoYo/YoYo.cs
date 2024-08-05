using UnityEngine;
using EventLibrary;
using EnumTypes;

public class YoYo : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // 요요가 적과 충돌했음을 알리는 이벤트 트리거
            EventManager<YoYoEvents>.TriggerEvent(YoYoEvents.YoYoAttached, collision.gameObject);
            this.transform.SetParent(collision.transform); // 요요를 적의 자식으로 설정하여 붙어있게 함
            this.GetComponent<Rigidbody>().isKinematic = true; // 요요가 더 이상 물리적으로 움직이지 않도록 설정
            Debug.Log("요요가 적에게 붙었습니다.");
        }
    }
}