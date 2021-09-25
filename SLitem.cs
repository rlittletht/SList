using System;
using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace SList
{
	// ============================================================================
	// S  L  I T E M
	// ============================================================================
	public class SLItem
	{
		public string Name { get; private set; }
		public Int64 Size { get; private set; }
		public string Path { get; private set; }
		public bool IsMarked { get; set; }
		public string Extension => System.IO.Path.GetExtension(Name);
		public bool Checked { get; set; }
		public bool CannotOpen { get; set; }
		public bool IsReparsePoint { get; private set; }

		// some items are intended only to matched *against*, but we shouldn't treat them as unique.  
		// (i.e. drive old and drive new.  we don't care to find dupes for items on drive new on 
		// drive new; we only care about finding items from drive old that are on drive new.

		public bool IsDestOnly { get; set; }

		FileInfo m_fi;
		DirectoryInfo m_di;
		private byte[] m_rgbSha256;

		public TextAtoms Atoms { get; set; }

		SLItem m_sliPrev;
		SLItem m_sliNext;

		public int CompareTo(SLItem sli)
		{
			return String.Compare(Path, sli.Path);
		}

		public SLItem Prev => m_sliPrev;
		public SLItem Next => m_sliNext;

		public enum SLItemCompare
		{
			CompareName = 1,
			CompareType = 2,
			CompareSize = 3,
			ComparePath = 4,
			CompareSizeDest,
			CompareHashkey
		}

		public string Hashkey => m_sFiName;

		private string m_sFiName;

		public SLItem()
		{
		}

		public byte[] Sha256Bytes => m_rgbSha256;

		static int GetHexVal(char hex)
		{
			int val = (int) hex;
			//For lowercase a-f letters:
			return val - (val <= '9' ? '0' : ('a' - 10));

			//For uppercase A-F letters:
			//return val - (val < 58 ? 48 : 55);
			//Or the two combined, but a bit slower:
			//return val - (val < 58 ? 48 : (val < 97 ? 55 : 97));
		}

		public static string GetPath(SLItem item) => item.Path;
		public static string SetPath(SLItem item, string value) => item.Path = value;
		public static string GetName(SLItem item) => item.Name;
		public static string SetName(SLItem item, string value) => item.Name = value;

		public static string GetItemHashKey(SLItem item) => item.Hashkey;
		public static string SetItemHashKey(SLItem item, string value) => item.m_sFiName = value;
		public static string GetIsReparsePoint(SLItem item) => item.IsReparsePoint ? "true" : null;
		public static void SetIsReparsePoint(SLItem item, string value) => item.IsReparsePoint = (value == null || value != "true") ? false : true;
		public static string GetSize(SLItem item) => item.Size.ToString();
		public static void SetSize(SLItem item, string value) => item.Size = Int64.Parse(value);

		public static void SetSha256(SLItem item, string value)
		{
			byte[] bytes = new byte[value.Length / 2];

			for (int i = 0; i < value.Length / 2; i++)
			{
				bytes[i] = (byte) ((GetHexVal(value[i * 2]) << 4) + (GetHexVal(value[(i * 2) + 1])));
			}

			item.m_rgbSha256 = bytes;
		}

		public static string GetSha256(SLItem item)
		{
			byte[] bytes = item.Sha256Bytes;

			if (bytes == null)
				return null;

			StringBuilder sb = new StringBuilder();

			foreach (byte b in bytes)
				sb.Append(b.ToString("x2"));

			return sb.ToString();
		}

		public void Rename(string newName)
		{
			Name = newName; // need to create a new hash key too, yes??
			throw new Exception("NYI: this is probably broken because the hash key is now out of sync");
		}

		public SLItem(string sName, long lSize, string sPath, string sFiName)
		{
			Name = sName;
			Size = lSize;
			Path = sPath;
			m_sFiName = sFiName;
			m_di = null;
			m_fi = null;
			Atoms = null;
		}

		public SLItem(string sName, long lSize, string sPath, DirectoryInfo di)
		{
			Name = sName;
			Size = lSize;
			Path = sPath;
			m_di = di;
			m_fi = null;
			Atoms = null;
		}

		public SLItem(string sName, long lSize, string sPath, FileInfo fi)
		{
			Name = sName;
			Size = lSize;
			Path = sPath;
			m_sFiName = String.Format("{0}/{1}", sPath, fi.Name);
			m_fi = fi;
			if ((m_fi.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
				IsReparsePoint = true;

			m_di = null;
			Atoms = null;
		}

		CultureInfo ci = new CultureInfo("en-US");

		public void ClearDupeChain()
		{
			m_sliNext = m_sliPrev = null;
		}

		public bool MatchesPathPrefix(string s)
		{
			if (Path.Length < s.Length)
				return false;

			if (Path.StartsWith(s, true /*ignoreCase*/, ci))
			{
				// make sure its a path subset, and not a part of another path
				if (Path.Length > s.Length
				    && Path[s.Length] != '\\'
				    && Path[s.Length] != '/')
				{
					return false;
				}

				return true;
			}

			return false;
		}

		public void AddDupeToChain(SLItem sli)
		{
			if (sli.m_sliNext != null || sli.m_sliPrev != null)
				throw new Exception("Can't add an sli that already has dupes!");

			SLItem sliLast = this;

			while (sliLast.m_sliNext != null)
				sliLast = sliLast.m_sliNext;

			sliLast.m_sliNext = sli;
			sli.m_sliPrev = sliLast;
		}


		public static int Compare(SLItem sli1, SLItem sli2, SLItemCompare slic, bool fReverse)
		{
			int n = 0;

			switch (slic)
			{
				case SLItemCompare.CompareHashkey:
					n = String.Compare(sli1.Hashkey, sli2.Hashkey);
					if (n == 0)
					{
						// they are again equivalent; the difference is now file size
						n = (int)(sli1.Size - sli2.Size);

						if (n == 0)
						{
							// yeesh.  diff is now folder

							n = String.Compare(sli1.Path, sli2.Path);
						}
					}
					break;
				case SLItemCompare.CompareType:
					n = string.Compare(System.IO.Path.GetExtension(sli1.Name), System.IO.Path.GetExtension(sli2.Name));
					if (n == 0)
						return Compare(sli1, sli2, SLItemCompare.CompareName, fReverse);
					break;
				case SLItemCompare.CompareName:
					n = String.Compare(sli1.Name, sli2.Name);
					if (n == 0)
					{
						// they are again equivalent; the difference is now file size
						n = (int)(sli1.Size - sli2.Size);

						if (n == 0)
						{
							// yeesh.  diff is now folder

							n = String.Compare(sli1.Path, sli2.Path);
						}
					}
					break;
				case SLItemCompare.CompareSize:
					n = (int)(sli1.Size - sli2.Size);

					if (n == 0)
					{
						// they are the same; now look at the name
						n = String.Compare(sli1.Name, sli2.Name);

						if (n == 0)
						{
							// yeesh.  diff is now folder

							n = String.Compare(sli1.Path, sli2.Path);
						}
					}
					break;
				case SLItemCompare.CompareSizeDest:
					n = (int)(sli1.Size - sli2.Size);

					if (n == 0)
					{
						// they are the same; now look at the name
						n = String.Compare(sli1.Name, sli2.Name);

						if (n == 0)
						{
							if (sli1.IsDestOnly == sli2.IsDestOnly)
								n = 0;
							else if (sli1.IsDestOnly)
								n = 1;
							else
								n = -1;

							if (n == 0)
							{
								// yeesh.  diff is now folder

								n = String.Compare(sli1.Path, sli2.Path);
							}
						}
					}
					break;
				case SLItemCompare.ComparePath:
					n = String.Compare(sli1.Path, sli2.Path);

					if (n == 0)
					{
						// they are equivalent; the difference is now based on the name
						n = String.Compare(sli1.Name, sli2.Name);

						if (n == 0)
						{
							// they are again equivalent; the difference is now file size
							n = (int)(sli1.Size - sli2.Size);
						}
					}
					break;
			}

			if (fReverse)
				return -n;
			else
				return n;
		}

		public void EnsureSha256()
		{
			if (m_rgbSha256 != null)
				return;

			using (SHA256 sha = SHA256.Create())
			{
				try
				{
					if (m_fi == null)
						m_fi = new FileInfo(System.IO.Path.Combine(Path, Name));

					using (FileStream fileStream = m_fi.OpenRead())
					{
						fileStream.Position = 0;
						m_rgbSha256 = sha.ComputeHash(fileStream);
						fileStream.Close();
					}
				}
				catch
				{ }
			}
		}

		public bool HasSha256 => m_rgbSha256 != null;
		public bool FCanCompareSha256(SLItem item)
		{
			EnsureSha256();
			item.EnsureSha256();

			return HasSha256 && item.HasSha256;
		}

		public bool IsEqualSha256(SLItem item)
		{
			if (m_rgbSha256.Length != item.m_rgbSha256.Length)
				return false;

			for (int i = 0; i < m_rgbSha256.Length; i++)
				if (m_rgbSha256[i] != item.m_rgbSha256[i])
					return false;

			return true;
		}
	};

}
