using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class BasicImageLevelExample : MonoBehaviour
{
	[System.Serializable]
	public class ColorToPrefabEntry
	{
		public Color32 color;
		public GameObject prefab;
	}

	[SerializeField]
	Texture2D m_levelData;
	[SerializeField]
	float m_cellSize = 1f;
	[SerializeField]
	List<ColorToPrefabEntry> m_prefabs;

	[Header("Fence")]
	[SerializeField]
	bool m_useFence = false;
	[SerializeField]
	Color32 m_fenceColor;
	[SerializeField]
	Color32 m_emptyColor;
	[SerializeField]
	GameObject m_fencePrefab;

	ImageAsData<Vector3> m_imageData;
	List<GameObject> m_loadedObjects = new List<GameObject>();

	void Start ()
	{
		m_imageData = new ImageAsData<Vector3>(m_levelData, new PixelCoordinateConverterFlatGrid(m_cellSize));
	}

	void OnGUI()
	{
		var rect = new Rect(30, 30, 150, 50);
		if (GUI.Button(rect, "Reload"))
		{
			Reload();
		}
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			Reload();
		}
	}
	
	void Clean()
	{
		foreach (var go in m_loadedObjects)
		{
			Destroy(go);
		}
		m_loadedObjects.Clear();
	}

	void Reload ()
	{
		Clean();

		m_imageData.ReadAllPixels(OnPixel);
	}

	void OnPixel(Color32 color, Vector3 position, PixelContext context)
	{
		var entry = m_prefabs.FirstOrDefault(e => e.color.Equals(color));

		if (entry != null)
		{
			var go = (GameObject)Instantiate(entry.prefab, position, Quaternion.identity);
			go.transform.parent = transform;
			m_loadedObjects.Add(go);

			if (color.Equals(m_fenceColor) && m_useFence)
			{
				bool down = context.Down.Equals(m_emptyColor);
				bool up = context.Up.Equals(m_emptyColor);
				bool left = context.Left.Equals(m_emptyColor);
				bool right = context.Right.Equals(m_emptyColor);

				if (down || up || left || right)
				{
					Quaternion rotation = Quaternion.identity;

					var fenceGO = (GameObject)Instantiate(m_fencePrefab, position, rotation);
					fenceGO.transform.parent = transform;
					m_loadedObjects.Add(fenceGO);
				}
			}
		}
	}
}
