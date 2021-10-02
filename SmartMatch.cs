using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace SList
{

	public enum TextAtomType
	{
		TitleWord,
		OtherWord,
		Separator
	};

	public class TextAtom
	{
		TextAtomType m_atomType;
		string m_sWord;
		char m_chSep;

		public TextAtomType TextAtomType
		{
			get { return m_atomType; }
			set { m_atomType = value; }
		}

		public TextAtom(TextAtomType atomType, string sWord)
		{
			// strip out ' and "
			int ich = 0, ichNext = 0, ichCur = 0, ichMax = sWord.Length;
			int ichL = sWord.IndexOf('\'');
			int ichQ = sWord.IndexOf('"');
			m_sWord = "";
			while (ichCur < ichMax)
			{
				if (ichQ < ichCur)
					ichQ = sWord.IndexOf('"', ichCur);
				if (ichL < ichCur)
					ichL = sWord.IndexOf('\'', ichCur);

				if (ichQ == -1)
					ichNext = ichL;
				else if (ichL == -1)
					ichNext = ichQ;
				else
					ichNext = Math.Min(ichQ, ichL);

				if (ichNext == -1)
					break;

				m_sWord += sWord.Substring(ich, ichNext - ich);
				ich = ichNext + 1;
				ichCur = ichNext + 1;
			}

			// figure out the next segment
			m_sWord += sWord.Substring(ich);

			m_atomType = atomType;
			m_chSep = '\0';
		}

		public TextAtom(TextAtomType atomType, char chSep)
		{
			m_sWord = null;
			m_atomType = atomType;
			m_chSep = chSep;
		}

		public bool FMatch(TextAtom textAtom)
		{
			if (m_atomType != textAtom.m_atomType)
				return false;

			if (m_atomType == TextAtomType.Separator)
				return true; // separators match, even if they're not the same

			return String.Compare(m_sWord, textAtom.m_sWord, true) == 0;
		}
	}

	public class TextAtoms // ATM Collection
	{
		List<TextAtom> m_atoms;
		int m_cWords;

		public int CountWords
		{
			get
			{
				if (m_cWords == -1) return (m_cWords = CTitleWordsInPlatm(m_atoms));
				else return m_cWords;
			}
		}


		public int Count
		{
			get { return m_atoms.Count; }
		}

		public TextAtoms(string sName)
		{
			m_cWords = -1;
			m_atoms = AtomsFromString(sName);
		}

		public TextAtom this[int i]
		{
			get { return (TextAtom)m_atoms[i]; }
			set { m_atoms[i] = value; }
		}

		/* P L A T M  B U I L D  F R O M  S T R I N G */
		/*----------------------------------------------------------------------------
		    %%Function: AtomsFromString
		    %%Qualified: SList.SListApp:TextAtoms.AtomsFromString
		    %%Contact: rlittle

		    Build atoms from the given sName
	    ----------------------------------------------------------------------------*/
		List<TextAtom> AtomsFromString(string sName)
		{
			List<TextAtom> atoms = new List<TextAtom>();
			int ich = 0;
			int ichFirst = -1;
			int ichMax;
			TextAtomType atomTypeCur = TextAtomType.TitleWord;
			bool fCollecting = false;
			bool fEndWord = false;

			if ((ich = sName.LastIndexOf('.')) != -1)
				sName = sName.Substring(0, ich);

			ich = 0;
			ichMax = sName.Length;

			while (ich < ichMax)
			{
				if (fEndWord)
				{
					// end the word and add it as atomTypeCur
					TextAtom textAtom = new TextAtom(atomTypeCur, sName.Substring(ichFirst, ich - ichFirst));
					atoms.Add(textAtom);
					fEndWord = false;
					fCollecting = false;
					ichFirst = -1;
				}

				// ok, classify the character
				char ch = sName[ich];

				if (Char.IsWhiteSpace(ch) && fCollecting)
				{
					fEndWord = true;
					continue; // this effectively pushes the token back on the stack
				}

				while (ich < ichMax && Char.IsWhiteSpace(sName[ich]))
					ich++;

				if (ich >= ichMax)
					break;

				ch = sName[ich];

				switch (ch)
				{
					case '(':
					case '[':
					case '{':
						if (fCollecting)
						{
							fEndWord = true;
							continue;
						}
						atomTypeCur = TextAtomType.OtherWord;
						break;
					case ')':
					case ']':
					case '}':
						if (fCollecting)
						{
							fEndWord = true;
							continue;
						}
						atomTypeCur = TextAtomType.TitleWord; // we don't handle nested parens, we always go back to the title
						break;
					case '-':
					case '=':
					case ':':
						if (fCollecting)
						{
							fEndWord = true;
							continue;
						}
						// all collected words before this now become OtherWord
						foreach (TextAtom atm in atoms)
						{
							if (atm.TextAtomType == TextAtomType.TitleWord)
								atm.TextAtomType = TextAtomType.OtherWord;
						}
						{
							TextAtom textAtom = new TextAtom(TextAtomType.Separator, ch);
							atoms.Add(textAtom);
						}
						break;
					case '\'':
					case '"':
					default:
						if (!fCollecting && (ch == '\'' || ch == '"'))
							break; // skip if we're not collecting

						// the rest start collecting
						if (!fCollecting)
						{
							ichFirst = ich;
							fCollecting = true;
						}
						break;
				}
				ich++;
			}
			if (fCollecting)
			{
				TextAtom textAtom = new TextAtom(atomTypeCur, sName.Substring(ichFirst, ich - ichFirst));
				atoms.Add(textAtom);
			}

			// before we're done, make sure there are *some* title words
			if (CTitleWordsInPlatm(atoms) == 0)
			{
				// work backwards and make title words until we hit a separator
				int i = atoms.Count - 1;

				while (i >= 0)
				{
					// skip trailing seps
					while (i >= 0 && ((TextAtom)atoms[i]).TextAtomType == TextAtomType.Separator)
						i--;

					if (i < 0)
						break;

					while (i >= 0 && ((TextAtom)atoms[i]).TextAtomType != TextAtomType.Separator)
						((TextAtom)atoms[i--]).TextAtomType = TextAtomType.TitleWord;

					break;
				}
			}
			return atoms;
		}


		/* C  T I T L E  W O R D S  I N  P L A T M */
		/*----------------------------------------------------------------------------
		    %%Function: CTitleWordsInPlatm
		    %%Qualified: SList.SListApp.CTitleWordsInPlatm
		    %%Contact: rlittle

	    ----------------------------------------------------------------------------*/
		int CTitleWordsInPlatm(List<TextAtom> atoms)
		{
			int c = 0;

			foreach (TextAtom atm in atoms)
			{
				if (atm.TextAtomType == TextAtomType.TitleWord)
					c++;
			}
			return c;
		}

		/* N  M A T C H  P L A T M */
		/*----------------------------------------------------------------------------
		    %%Function: NMatchPlatm
		    %%Qualified: SList.SLItem.NMatchPlatm
		    %%Contact: rlittle

		    try to match the two platms.  Return the confidence (0-100%)

		    (confidence is the number of matched words / the number of words in platm1)
	    ----------------------------------------------------------------------------*/
		public int NMatch(TextAtoms textAtoms)
		{
			int i1 = this.Count - 1, i2 = textAtoms.Count - 1;
			int cMatch = 0;

			if (i1 < 0 || i2 < 0)
				return cMatch;

			while (i1 >= 0 && i2 >= 0)
			{
				// find the first titleword
				while (i2 >= 0 && textAtoms[i2].TextAtomType != TextAtomType.TitleWord)
					i2--;

				if (i2 < 0)
					break;

				while (i1 >= 0 && this[i1].TextAtomType != TextAtomType.TitleWord)
					i1--;

				if (i1 < 0)
					break;

				if (!this[i1].FMatch(textAtoms[i2]))
					break;

				cMatch++;
				i1--;
				i2--;
			}

			return (cMatch * 100) / this.CountWords;
		}
	}
}