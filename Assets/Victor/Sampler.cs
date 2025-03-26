public class Sampler : MonoBehaviour
{
    public TMPro.TextMeshProUGUI Sampler;
    // Reference a MonoBehaviour that implements IBrain (assign this in the Unity Inspector)
    public MonoBehaviour BrainComponent;

    private IBrain brain;

    void Start()
    {
        Sampler.text = "";
        brain = BrainComponent as IBrain;
        if (brain == null)
        {
            Debug.LogError("Assigned BrainComponent does not implement IBrain!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CanBeTaken") && brain != null)
        {
            string chosenSample = brain.DecideSample(other.gameObject);
            Sampler.text = "Touched: " + chosenSample;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Sampler.text = "";
    }
}
