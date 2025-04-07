using UnityEngine;

public class TrackPainter : MonoBehaviour
{
	public RenderTexture trackMaskRT;  // Assign your TrackMaskRT here.
	public Texture2D brushTexture;     // A simple circular brush texture.
	public float brushSize = 10f;        // In UV space (or adjusted based on terrain size).
	public LayerMask terrainLayer;     // Make sure this includes your terrain.

	// A material with a brush shader that “stamps” the brush texture.
	public Material brushMaterial;

	void Update()
	{
		// For each rover wheel, perform a raycast downward.
		RaycastHit hit;
		if (Physics.Raycast(transform.position, -Vector3.up, out hit, 5f, terrainLayer))
		{
			// Ensure the hit collider is your terrain.
			Terrain terrain = hit.collider.GetComponent<Terrain>();
			if (terrain != null)
			{
				// Convert the hit point to UV coordinates.
				Vector3 terrainLocalPos = hit.point - terrain.transform.position;
				Vector3 terrainSize = terrain.terrainData.size;
				float u = Mathf.Clamp01(terrainLocalPos.x / terrainSize.x);
				float v = Mathf.Clamp01(terrainLocalPos.z / terrainSize.z);
				Vector2 uv = new Vector2(u, v);

				// Stamp the brush at the UV coordinate.
				print("Painting at UV: " + uv);
				PaintAtUV(uv);
			}
		}
	}

	void PaintAtUV(Vector2 uv)
	{
		// Pass the UV coordinate and brush size to the brush material.
		brushMaterial.SetVector("_BrushPos", new Vector4(uv.x, uv.y, 0, 0));
		brushMaterial.SetFloat("_BrushSize", brushSize);
		// Optionally adjust opacity.
		brushMaterial.SetFloat("_Opacity", 1.0f);
		brushMaterial.SetTexture("_BrushTex", brushTexture);

		// Update the track mask RenderTexture.
		// Use a temporary RenderTexture to blend the new brush stroke.
		RenderTexture tempRT = RenderTexture.GetTemporary(trackMaskRT.width, trackMaskRT.height, 0, trackMaskRT.format);
		// Copy the current state.
		Graphics.Blit(trackMaskRT, tempRT);
		// Now "stamp" the brush onto the temporary texture.
		Graphics.Blit(tempRT, trackMaskRT, brushMaterial);
		RenderTexture.ReleaseTemporary(tempRT);
	}
}
