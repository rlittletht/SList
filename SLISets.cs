using System;
using System.Collections.Generic;
using NUnit.Framework.Internal.Execution;
using TCore.XmlSettings;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace SList
{
    public class SLISets
    {
        public List<SLISet> Sets { get; set; }

        public SLISets(SLISet source, SLISet destination)
        {
            source.FilesListItemEnumerator = null;
            destination.FilesListItemEnumerator = null;
            Sets = new List<SLISet>(new [] {source, destination});
        }

        public SLISets()
        {
            Sets = new List<SLISet>();
        }

        public IEnumerator<SLISet> SetItemEnumerator { get; set; }

        static RepeatContext<SLISets>.RepeatItemContext CreateSetRepeatItemContext(
            SLISets sets,
            Element<SLISets> element,
            RepeatContext<SLISets>.RepeatItemContext parent)
        {
            // for write...
            if (sets != null && sets.SetItemEnumerator != null)
            {
                return new RepeatContext<SLISets>.RepeatItemContext(
                    element,
                    parent,
                    sets.SetItemEnumerator.Current);
            }

            // for read
            return new RepeatContext<SLISets>.RepeatItemContext(element, parent, new SLISet());
        }

        static bool AreRemainingSets(SLISets sets, RepeatContext<SLISets>.RepeatItemContext itemcontext)
        {
            if (sets == null || sets.Sets == null || sets.Sets.Count == 0)
                return false;

            if (sets.SetItemEnumerator == null)
                sets.SetItemEnumerator = sets.Sets.GetEnumerator();

            return sets.SetItemEnumerator.MoveNext();
        }

        private static void CommitSetItemContext(SLISets sets, RepeatContext<SLISets>.RepeatItemContext itemcontext)
        {
            SLISet set = (SLISet)itemcontext.RepeatKey;
            if (sets.Sets == null)
                sets.Sets = new List<SLISet>();

            sets.Sets.Add(set);
        }

        private static void SetSourceName(SLISets t, string value, RepeatContext<SLISets>.RepeatItemContext repeatitemcontext) =>
            ((SLISet)repeatitemcontext.RepeatKey).SetTypeFromState(value);

        private static string GetSourceName(SLISets t, RepeatContext<SLISets>.RepeatItemContext repeatitemcontext) =>
            ((SLISet)repeatitemcontext.RepeatKey).Name;

        static RepeatContext<SLISets>.RepeatItemContext CreateFileRepeatItemContext(
            SLISets sets,
            Element<SLISets> element,
            RepeatContext<SLISets>.RepeatItemContext parent)
        {
            SLISet set = ((SLISet)parent.RepeatKey);

            // for write...
            if (set.ItemsInternal != null && set.FilesListItemEnumerator != null)
            {
                return new RepeatContext<SLISets>.RepeatItemContext(
                    element,
                    parent,
                    set.ItemsInternal[set.FilesListItemEnumerator.Current]);
            }

            // for read
            return new RepeatContext<SLISets>.RepeatItemContext(element, parent, new SLItem());
        }

        static bool AreRemainingFiles(SLISets sets, RepeatContext<SLISets>.RepeatItemContext itemcontext)
        {
            SLISet set = ((SLISet)itemcontext.RepeatKey);

            if (set.ItemsInternal == null || set.ItemsInternal.Count == 0)
                return false;

            if (set.FilesListItemEnumerator == null)
                set.FilesListItemEnumerator = set.ItemsInternal.Keys.GetEnumerator();

            return set.FilesListItemEnumerator.MoveNext();
        }

        private static void CommitFileRepeatItemContext(SLISets sets, RepeatContext<SLISets>.RepeatItemContext itemcontext)
        {
            SLItem item = ((SLItem)itemcontext.RepeatKey);
            SLISet set = ((SLISet)itemcontext.Parent.RepeatKey);

            if (set.ItemsInternal == null)
                throw new Exception("should always have allocated in our constructor");

            set.AddInternal(item); // don't add to the list view yet...
        }

  		private static void SetSha256(SLISets t, string value, RepeatContext<SLISets>.RepeatItemContext repeatitemcontext) => SLItem.SetSha256((SLItem)repeatitemcontext.RepeatKey, value);
		private static void SetItemSize(SLISets t, string value, RepeatContext<SLISets>.RepeatItemContext repeatitemcontext) => SLItem.SetSize(((SLItem) repeatitemcontext.RepeatKey), value);
        private static void SetItemHashKey(SLISets t, string value, RepeatContext<SLISets>.RepeatItemContext repeatitemcontext)	=> SLItem.SetItemHashKey((SLItem)repeatitemcontext.RepeatKey, value);
        private static void SetName(SLISets t, string value, RepeatContext<SLISets>.RepeatItemContext repeatitemcontext)	=> SLItem.SetName((SLItem)repeatitemcontext.RepeatKey, value);
		private static void SetPath(SLISets t, string value, RepeatContext<SLISets>.RepeatItemContext repeatitemcontext)	=> SLItem.SetPath((SLItem)repeatitemcontext.RepeatKey, value);
		private static void SetIsReparsePoint(SLISets t, string value, RepeatContext<SLISets>.RepeatItemContext repeatitemcontext) => SLItem.SetIsReparsePoint((SLItem)repeatitemcontext.RepeatKey, value);
        private static void SetIsMarked(SLISets t, string value, RepeatContext<SLISets>.RepeatItemContext repeatitemcontext) => SLItem.SetIsMarked(((SLItem)repeatitemcontext.RepeatKey), value);
        private static void SetChecked(SLISets t, string value, RepeatContext<SLISets>.RepeatItemContext repeatitemcontext) => SLItem.SetChecked(((SLItem)repeatitemcontext.RepeatKey), value);
        private static void SetCannotOpen(SLISets t, string value, RepeatContext<SLISets>.RepeatItemContext repeatitemcontext) => SLItem.SetCannotOpen(((SLItem)repeatitemcontext.RepeatKey), value);

        private static string GetIsReparsePoint(SLISets t, RepeatContext<SLISets>.RepeatItemContext repeatitemcontext) => SLItem.GetIsReparsePoint((SLItem)repeatitemcontext.RepeatKey);
        private static string GetItemHashKey(SLISets t, RepeatContext<SLISets>.RepeatItemContext repeatitemcontext) => SLItem.GetItemHashKey((SLItem)repeatitemcontext.RepeatKey);
        private static string GetSha256(SLISets t, RepeatContext<SLISets>.RepeatItemContext repeatitemcontext) => SLItem.GetSha256((SLItem)repeatitemcontext.RepeatKey);
        private static string GetItemSize(SLISets t, RepeatContext<SLISets>.RepeatItemContext repeatitemcontext) => SLItem.GetSize((SLItem)repeatitemcontext.RepeatKey);
        private static string GetName(SLISets t, RepeatContext<SLISets>.RepeatItemContext repeatitemcontext) => SLItem.GetName((SLItem)repeatitemcontext.RepeatKey);
        private static string GetPath(SLISets t, RepeatContext<SLISets>.RepeatItemContext repeatitemcontext) => SLItem.GetPath((SLItem)repeatitemcontext.RepeatKey);
        private static string GetIsMarked(SLISets t, RepeatContext<SLISets>.RepeatItemContext repeatitemcontext) => SLItem.GetIsMarked((SLItem)repeatitemcontext.RepeatKey);
        private static string GetChecked(SLISets t, RepeatContext<SLISets>.RepeatItemContext repeatitemcontext) => SLItem.GetChecked((SLItem)repeatitemcontext.RepeatKey);
        private static string GetCannotOpen(SLISets t, RepeatContext<SLISets>.RepeatItemContext repeatitemcontext) => SLItem.GetCannotOpen((SLItem)repeatitemcontext.RepeatKey);

        static XmlDescription<SLISets> CreateXmlDescriptionForState()
        {
            return XmlDescriptionBuilder<SLISets>
               .Build("http://www.thetasoft.com/scehmas/SList/state/2024", "State")
               .DiscardAttributesWithNoSetter()
               .DiscardUnknownAttributes()
               .AddChildElement("Set")
               .SetRepeating(
                    CreateSetRepeatItemContext,
                    AreRemainingSets,
                    CommitSetItemContext)
               .AddAttribute("Source", GetSourceName, SetSourceName)
               .AddChildElement("File", null, null)
               .SetRepeating(
                    CreateFileRepeatItemContext,
                    AreRemainingFiles,
                    CommitFileRepeatItemContext)
               .AddAttribute("hashKey", GetItemHashKey, SetItemHashKey)
               .AddAttribute("size", GetItemSize, SetItemSize)
               .AddAttribute("isReparsePoint", GetIsReparsePoint, SetIsReparsePoint)
               .AddAttribute("isMarked", GetIsMarked, SetIsMarked)
               .AddAttribute("isChecked", GetChecked, SetChecked)
               .AddAttribute("isCannotOpen", GetCannotOpen, SetCannotOpen)
               .AddChildElement("sha256", GetSha256, SetSha256)
               .AddElement("name", GetName, SetName)
               .AddElement("path", GetPath, SetPath);
        }

        public static void SaveState(SLISet source, SLISet destination, string outfile)
        {
            SLISets sets = new SLISets(source, destination);

            XmlDescription<SLISets> xml = CreateXmlDescriptionForState();

            using (WriteFile<SLISets> writeFile = WriteFile<SLISets>.CreateSettingsFile(xml, outfile, sets))
            {
                writeFile.SerializeSettings(xml, sets);
            }
        }

        public static void LoadState(string infile, out SLISet source, out SLISet destination)
        {
            SLISets sets = new SLISets();

            XmlDescription<SLISets> xml = CreateXmlDescriptionForState();

            using (ReadFile<SLISets> readFile = ReadFile<SLISets>.CreateSettingsFile(infile))
            {
                readFile.DeSerialize(xml, sets);
            }

            source = sets.Sets[0];
            destination = sets.Sets[1];
        }
    }
}
