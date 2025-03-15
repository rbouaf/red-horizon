using UnityEngine;

public class Minimap : MonoBehaviour
{
    public GameObject player;
    public Vector3 offset = new Vector3(0, 40, 0);

    void Start(){
        player =  GameObject.FindGameObjectWithTag("Player");
    }

    void LateUpdate(){
        Vector3 playerPosition = player.transform.position;
        transform.position = new Vector3(playerPosition.x, playerPosition.y + offset.y, playerPosition.z);
    }
}
