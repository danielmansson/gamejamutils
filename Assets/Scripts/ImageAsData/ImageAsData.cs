using UnityEngine;
using System.Collections;
using System;


public class ImageAsData<T>
{
	public abstract class PixelCoordinateConverter
	{
		public abstract T Convert(int x, int y, int w, int h);
	}

	Texture2D m_texture;
	PixelCoordinateConverter m_converter;

	public ImageAsData(Texture2D texture, PixelCoordinateConverter coordinateConverter)
	{
		m_texture = texture;
		m_converter = coordinateConverter;
	}

	public void ReadAllPixels(System.Action<Color32, T, PixelContext> pixelWorker)
	{
		var pixels = m_texture.GetPixels32();
		int w = m_texture.width;
		int h = m_texture.height;

		for (int i = 0; i < pixels.Length; i++)
		{
			int x = i % w;
			int y = i / w;

			pixelWorker(pixels[i], m_converter.Convert(x, y, w, h), new PixelContext(pixels, x, y, w, h));
		}
	}
}

public class PixelCoordinateConverterFlatGrid : ImageAsData<Vector3>.PixelCoordinateConverter
{
	float m_cellSize;

	public PixelCoordinateConverterFlatGrid(float cellSize)
	{
		m_cellSize = cellSize;
	}

	public override Vector3 Convert(int x, int y, int w, int h)
	{
		Vector3 offset = new Vector3((w - 1) * 0.5f, 0f, (h - 1) * 0.5f);
		Vector3 pos = new Vector3(x, 0, y);

		return m_cellSize * (pos - offset);
	}
}

public class PixelContext
{
	Color32[] m_pixels;
	int m_x;
	int m_y;
	int m_w;
	int m_h;

	public PixelContext(Color32[] pixels, int x, int y, int w, int h)
	{
		m_pixels = pixels;
		m_x = x;
		m_y = y;
		m_w = w;
		m_h = h;
	}

	public Color32 GetRelative(int dx, int dy)
	{
		int x = m_x + dx;
		int y = m_y + dy;

		if (x < 0 || x >= m_w || y < 0 || y >= m_h)
		{
			return new Color32(0, 0, 0, 255);
		}
		else
		{
			return m_pixels[y * m_w + x];
		}
	}

	public Color32 Up { get { return GetRelative(0, -1); } }
	public Color32 Down { get { return GetRelative(0, 1); } }
	public Color32 Left { get { return GetRelative(-1, 0); } }
	public Color32 Right { get { return GetRelative(1, 0); } }
}
