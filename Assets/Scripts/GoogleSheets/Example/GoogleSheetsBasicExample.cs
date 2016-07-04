using UnityEngine;
using System.Collections;

public class GoogleSheetsBasicExample : MonoBehaviour
{
	[SerializeField]
	GoogleSheetsSystem m_sheetsSystem;

	GoogleSheets m_sheets;

	void Start()
	{
		m_sheets = m_sheetsSystem.Sheets;

		m_sheets.OnUpdate += OnSheetsUpdated;
	}

	void OnDestroy()
	{
		m_sheets.OnUpdate -= OnSheetsUpdated;
	}

	void OnGUI()
	{
		var configs = m_sheets.AvailableConfigurationVariants;
		var rect = new Rect(0, 0, 200, 25);

		rect.x = 50;
		for (int i = 0; i < configs.Count; i++)
		{
			rect.y = 40 + i * 30;
			if (GUI.Button(rect, configs[i]))
			{
				m_sheets.CurrentConfigurationVariant = configs[i];
			}
		}

		rect.y = 10;
		GUI.Label(rect, "Active config: " + m_sheets.CurrentConfigurationVariant);

		rect.y = 40;
		rect.width = 110;
		foreach (var kvp in m_sheets.CurrentDataSet)
		{
			rect.x = 300;
			GUI.Label(rect, kvp.Key);
			rect.x = 300 + rect.width;
			GUI.Label(rect, kvp.Value);

			rect.y += 20;
		}
	}

	void Update()
	{

	}

	void OnSheetsUpdated()
	{

	}
}
