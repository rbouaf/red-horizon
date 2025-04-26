using UnityEngine;

public class Minimap : MonoBehaviour
{
    public GameObject player;

    void Start(){
        player =  GameObject.FindGameObjectWithTag("Player");
        if (player == null) {
             Debug.LogError("Player object not found!");
        }
    }

    void LateUpdate(){
        Vector3 playerPosition = player.transform.position;
        transform.position = new Vector3(playerPosition.x, 140, playerPosition.z);
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
