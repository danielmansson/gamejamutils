using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GoogleSheetsSystem : MonoBehaviour
{
	[SerializeField]
	string m_googleSheetId = "";
	[SerializeField]
	bool m_updateOnStart = true;
	[SerializeField]
	bool m_autoUpdate = true;
	[SerializeField]
	float m_autoUpdateFrequency = 5f;
	[SerializeField]
	string m_startConfig = "default";

	public GoogleSheets Sheets { get; private set; }

	float m_autoUpdateTimer;

	void Awake()
	{
		Sheets = new GoogleSheets(m_googleSheetId, new UnityWWWProvider());
		Sheets.CurrentConfigurationVariant = m_startConfig;

		if (m_updateOnStart)
		{
			Sheets.UpdateData((success) => { });
		}
	}

	void Update()
	{
		if (m_autoUpdate)
		{
			m_autoUpdateTimer += Time.unscaledDeltaTime;
			if (m_autoUpdateTimer > m_autoUpdateFrequency)
			{
				m_autoUpdateTimer = 0f;
				Sheets.UpdateData((success) =>
				{
					//Don't really care
				});
			}
		}
	}
}


