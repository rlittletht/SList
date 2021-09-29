using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using NUnit.Framework;
using TCore.XmlSettings;

namespace SList
{
	public class PreferredPaths : IList
	{
		public class PathItem
		{
			public string Path { get; set; }
			public static string SetPath(PathItem item, string value) => item.Path = value;
			public static string GetPath(PathItem item) => item.Path;

			public override string ToString() => Path;
		}

		public List<PathItem> Paths { get; set; }

		public PreferredPaths()
		{
			Paths = new List<PathItem>();
		}

		public void Add(string path)
		{
			Paths.Add(new PathItem() {Path = path});
		}

		int GetIndex(string path)
		{
			for (int i = 0; i < Paths.Count; i++)
				if (String.Compare(Paths[i].Path, path, true) == 0)
					return i;

			return -1;
		}

		public bool Remove(string path)
		{
			int i = GetIndex(path);
			if (i != -1)
			{
				Paths.RemoveAt(i);
				return true;
			}
			return false;
		}

		public IEnumerator<PathItem> ItemEnumerator { get; set; }

		static RepeatContext<PreferredPaths>.RepeatItemContext CreatePathRepeatItemContext(
			PreferredPaths paths,
			Element<PreferredPaths> element,
			RepeatContext<PreferredPaths>.RepeatItemContext parent)
		{
			// for write...
			if (paths.Paths != null && paths.ItemEnumerator != null)
			{
				return new RepeatContext<PreferredPaths>.RepeatItemContext(
					element,
					parent,
					paths.ItemEnumerator.Current);
			}

			// for read
			return new RepeatContext<PreferredPaths>.RepeatItemContext(element, parent, new PathItem());
		}

		static bool AreRemainingPaths(PreferredPaths t, RepeatContext<PreferredPaths>.RepeatItemContext itemcontext)
		{
			if (t.Paths == null || t.Paths.Count == 0)
				return false;

			if (t.ItemEnumerator == null)
				t.ItemEnumerator = t.Paths.GetEnumerator();

			return t.ItemEnumerator.MoveNext();
		}

		private static void CommitPathRepeatItemContext(PreferredPaths t, RepeatContext<PreferredPaths>.RepeatItemContext itemcontext)
		{
			PathItem item = (PathItem)itemcontext.RepeatKey;
			if (t.Paths == null)
				t.Paths = new List<PathItem>();

			t.Paths.Add(item); // don't add to the list view yet...
		}

		static XmlDescription<PreferredPaths> CreateXmlDescription()
		{
			return XmlDescriptionBuilder<PreferredPaths>
				.Build("http://www.thetasoft.com/scehmas/SList/preferredPathList/2020", "PreferredPathList")
				.DiscardAttributesWithNoSetter()
				.DiscardUnknownAttributes()
				.AddChildElement("Path", null, null)
				.SetRepeating(
					CreatePathRepeatItemContext,
					AreRemainingPaths,
					CommitPathRepeatItemContext)
				.AddAttribute("path", GetPath, SetPath);
		}

		public static void SavePreferredPathListXml(PreferredPaths paths, string outfile)
		{
			XmlDescription<PreferredPaths> xml = CreateXmlDescription();

			paths.ItemEnumerator = null;
			using (WriteFile<PreferredPaths> writeFile = WriteFile<PreferredPaths>.CreateSettingsFile(xml, outfile, paths))
				writeFile.SerializeSettings(xml, paths);
		}

		public static void LoadPreferredPathListXml(PreferredPaths paths, string infile)
		{
			XmlDescription<PreferredPaths> xml = CreateXmlDescription();

			using (ReadFile<PreferredPaths> readFile = ReadFile<PreferredPaths>.CreateSettingsFile(infile))
				readFile.DeSerialize(xml, paths);
		}

		private static void SetPath(PreferredPaths t, string value, RepeatContext<PreferredPaths>.RepeatItemContext repeatitemcontext) => PathItem.SetPath((PathItem)repeatitemcontext.RepeatKey, value);
		private static string GetPath(PreferredPaths t, RepeatContext<PreferredPaths>.RepeatItemContext repeatitemcontext) => PathItem.GetPath((PathItem)repeatitemcontext.RepeatKey);

		#region IList implementation

		public IEnumerator<PathItem> GetEnumerator() => ItemEnumerator;
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public int Add(object item)
		{
			Paths.Add((PathItem) item);
			return Paths.Count - 1;
		}

		public bool Contains(object value)
		{
			throw new NotImplementedException();
		}

		public void Clear() => Paths.Clear();
		public void Remove(object item) => Remove(((PathItem)item).Path);
		public void CopyTo(Array array, int index)
		{
			throw new NotImplementedException();
		}

		public int Count => Paths.Count;
		public object SyncRoot => this;
		public bool IsSynchronized => false;
		public bool IsReadOnly => false;
		public bool IsFixedSize => false;
		public int IndexOf(object item) => GetIndex(((PathItem)item).Path);
		public void Insert(int index, object item) => Paths.Insert(index, (PathItem)item);
		public void RemoveAt(int index) => Paths.RemoveAt(index);

		object IList.this[int index]
		{
			get => Paths[index];
			set => Paths[index] = (PathItem)value;
		}

		#endregion

	}
}