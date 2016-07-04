using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GoogleSheets
{
	const string URL = "https://spreadsheets.google.com/feeds/cells/{0}/od6/public/basic?alt=json";

	//Dictionary with 'variant' -> 'data key', 'data value'
	Dictionary<string, Dictionary<string, string>> m_data = new Dictionary<string, Dictionary<string, string>>();

	WWWProvider m_wwwProvider;

	string m_currentConfigurationVariant = "default";
	string m_sheetId = "";
	bool m_updateRequestInProgress;

	public GoogleSheets(string sheetId, WWWProvider wwwProvider)
	{
		m_wwwProvider = wwwProvider;
		m_sheetId = sheetId;
		LoadCache();
	}

	#region Public interface
	public event Action OnUpdate;

	public void UpdateData(System.Action<bool> onDone = null)
	{
		if (m_updateRequestInProgress)
		{
			onDone(false);
		}
		else
		{
			m_updateRequestInProgress = true;
			string url = string.Format(URL, m_sheetId);
			m_wwwProvider.HttpGet(url, (wwwResult) =>
			{
				bool success = UpdateData(wwwResult);
				onDone(success);
				m_updateRequestInProgress = false;

				if (OnUpdate != null && success)
				{
					OnUpdate();
				}
			});
		}
	}

	public string CurrentConfigurationVariant
	{
		get { return m_currentConfigurationVariant; }
		set { m_currentConfigurationVariant = value; }
	}

	public List<string> AvailableConfigurationVariants
	{
		get { return new List<string>(m_data.Keys); }
	}

	public Dictionary<string, string> CurrentDataSet
	{
		get
		{
			Dictionary<string, string> result;
			if (m_data.TryGetValue(CurrentConfigurationVariant, out result))
				return result;
			else
				return new Dictionary<string, string>();
		}
	}

	public string GetString(string key, string defaultValue = "")
	{
		Dictionary<string, string> data;
		if (m_data.TryGetValue(m_currentConfigurationVariant, out data))
		{
			string result;
			if (data.TryGetValue(key, out result))
			{
				return result;
			}
		}

		return defaultValue;
	}

	public int GetInt(string key, int defaultValue = 0)
	{
		string data = GetString(key, null);

		if (data != null)
		{
			int result;
			if (int.TryParse(data, out result))
			{
				return result;
			}
		}

		return defaultValue;
	}

	public float GetFloat(string key, float defaultValue = 0)
	{
		string data = GetString(key, null);

		if (data != null)
		{
			float result;
			if (float.TryParse(data, out result))
			{
				return result;
			}
		}

		return defaultValue;
	}
	#endregion

	#region UpdateData helpers
	bool UpdateData(WWWProviderResult www)
	{
		if (www.error != null)
		{
			Debug.LogError("Failed to update data from google sheets: " + www.error);
			return false;
		}
		else
		{
			UpdateValues(www.text);
			return true;
		}
	}

	string GetCellId(int column, int row)
	{
		return ((char)('A' + column - 1)).ToString() + row.ToString();
	}

	void UpdateValues(string jsonData)
	{
		Dictionary<string, string> cells = new Dictionary<string, string>();

		//Json parsing
		//Parse all cells
		{
			var root = (Dictionary<string, object>)MiniJSON.Json.Deserialize(jsonData);
			var feed = (Dictionary<string, object>)root["feed"];
			var entries = (List<object>)feed["entry"];

			foreach (var entryObj in entries)
			{
				var entry = (Dictionary<string, object>)entryObj;

				var title = (Dictionary<string, object>)entry["title"];
				string key = (string)title["$t"];

				var content = (Dictionary<string, object>)entry["content"];
				string value = (string)content["$t"];

				cells.Add(key, value);
			}
		}

		//Convert cells into our data format
		//First row is configuration variant
		//First column is data keys
		{
			//Find number of configuration variants
			int numVariants = 0;
			while (true)
			{
				string cellId = GetCellId(2 + numVariants, 1);
				if (!cells.ContainsKey(cellId))
					break;
				++numVariants;
			}

			//Find number of data keys
			int numDataEntries = 0;
			while (true)
			{
				string cellId = GetCellId(1, 2 + numDataEntries);
				if (!cells.ContainsKey(cellId))
					break;
				++numDataEntries;
			}

			//Clear the old data
			m_data = new Dictionary<string, Dictionary<string, string>>();

			//Fill in the data
			for (int i = 0; i < numVariants; i++)
			{
				string variantCellId = GetCellId(2 + i, 1);
				string variantName = cells[variantCellId];

				if (string.IsNullOrEmpty(variantName))
				{
					//Skip it
				}
				else
				{
					var variantData = new Dictionary<string, string>();
					m_data.Add(variantName, variantData);

					for (int j = 0; j < numDataEntries; j++)
					{
						string keyCellId = GetCellId(1, 2 + j);
						string valueCellId = GetCellId(2 + i, 2 + j);

						string keyStr;
						string valueStr;

						if (cells.TryGetValue(keyCellId, out keyStr) && cells.TryGetValue(valueCellId, out valueStr))
						{
							variantData.Add(keyStr, valueStr);
						}
					}
				}
			}
		}

		SaveCache();
	}
	#endregion

	#region Local storage cache
	static string CACHE_PATH = Application.persistentDataPath + "/GoogleSheetsCache.txt";

	void SaveCache()
	{
		var jsonStr = MiniJSON.Json.Serialize(m_data);
		System.IO.File.WriteAllText(CACHE_PATH, jsonStr);
	}

	void LoadCache()
	{
		string jsonStr = null;
		try
		{
			jsonStr = System.IO.File.ReadAllText(CACHE_PATH);
		}
		catch (System.Exception)
		{
			//Failed to open file or something with IO
		}

		if (jsonStr != null)
		{
			var root = (Dictionary<string, object>)MiniJSON.Json.Deserialize(jsonStr);

			m_data = new Dictionary<string, Dictionary<string, string>>();
			foreach (var kvp in root)
			{
				var entryData = (Dictionary<string, object>)kvp.Value;
				var configVariant = new Dictionary<string, string>();

				foreach (var entryKVP in entryData)
				{
					configVariant.Add(entryKVP.Key, (string)entryKVP.Value);
				}

				m_data.Add(kvp.Key, configVariant);
			}
		}
	}
	#endregion
}