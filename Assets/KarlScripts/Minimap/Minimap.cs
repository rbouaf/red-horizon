using UnityEngine;

public class Minimap : MonoBehaviour
{
    public GameObject player;
    //public Vector3 offset = new Vector3(0, 40, 0);

    //public float fixedHeight = 40f;


    void Start(){
        player =  GameObject.FindGameObjectWithTag("Player");
        if (player == null) {
             Debug.LogError("Rover parent object not found!");
        }
    }

    void LateUpdate(){
        Vector3 playerPosition = player.transform.position;
        transform.position = new Vector3(playerPosition.x, 140, playerPosition.y);
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
