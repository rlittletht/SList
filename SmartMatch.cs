using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Globalization;
using System.Media;
using System.Resources;

namespace SList
{

	public enum ATMT
	{
		TitleWord,
		OtherWord,
		Separator
	};

	public class ATM
	{
		ATMT m_atmt;
		string m_sWord;
		char m_chSep;

		public ATMT Atmt
		{
			get { return m_atmt; }
			set { m_atmt = value; }
		}

		public ATM(ATMT atmt, string sWord)
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

			m_atmt = atmt;
			m_chSep = '\0';
		}

		public ATM(ATMT atmt, char chSep)
		{
			m_sWord = null;
			m_atmt = atmt;
			m_chSep = chSep;
		}

		public bool FMatch(ATM atm)
		{
			if (m_atmt != atm.m_atmt)
				return false;

			if (m_atmt == ATMT.Separator)
				return true; // separators match, even if they're not the same

			return String.Compare(m_sWord, atm.m_sWord, true) == 0;
		}
	}

	public class ATMC // ATM Collection
	{
		ArrayList m_platm;
		int m_cWords;

		public int CountWords
		{
			get
			{
				if (m_cWords == -1) return (m_cWords = CTitleWordsInPlatm(m_platm));
				else return m_cWords;
			}
		}


		public int Count
		{
			get { return m_platm.Count; }
		}

		public ATMC(string sName)
		{
			m_cWords = -1;
			m_platm = PlatmBuildFromString(sName);
		}

		public ATM this[int i]
		{
			get { return (ATM)m_platm[i]; }
			set { m_platm[i] = value; }
		}

		/* P L A T M  B U I L D  F R O M  S T R I N G */
		/*----------------------------------------------------------------------------
		    %%Function: PlatmBuildFromString
		    %%Qualified: SList.SListApp:ATMC.PlatmBuildFromString
		    %%Contact: rlittle

		    Build platm from the given sName
	    ----------------------------------------------------------------------------*/
		ArrayList PlatmBuildFromString(string sName)
		{
			ArrayList platm = new ArrayList();
			int ich = 0;
			int ichFirst = -1;
			int ichMax;
			ATMT atmtCur = ATMT.TitleWord;
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
					// end the word and add it as atmtCur
					ATM atm = new ATM(atmtCur, sName.Substring(ichFirst, ich - ichFirst));
					platm.Add(atm);
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
						atmtCur = ATMT.OtherWord;
						break;
					case ')':
					case ']':
					case '}':
						if (fCollecting)
						{
							fEndWord = true;
							continue;
						}
						atmtCur = ATMT.TitleWord; // we don't handle nested parens, we always go back to the title
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
						foreach (ATM atm in platm)
						{
							if (atm.Atmt == ATMT.TitleWord)
								atm.Atmt = ATMT.OtherWord;
						}
						{
							ATM atm = new ATM(ATMT.Separator, ch);
							platm.Add(atm);
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
				ATM atm = new ATM(atmtCur, sName.Substring(ichFirst, ich - ichFirst));
				platm.Add(atm);
			}

			// before we're done, make sure there are *some* title words
			if (CTitleWordsInPlatm(platm) == 0)
			{
				// work backwards and make title words until we hit a separator
				int i = platm.Count - 1;

				while (i >= 0)
				{
					// skip trailing seps
					while (i >= 0 && ((ATM)platm[i]).Atmt == ATMT.Separator)
						i--;

					if (i < 0)
						break;

					while (i >= 0 && ((ATM)platm[i]).Atmt != ATMT.Separator)
						((ATM)platm[i--]).Atmt = ATMT.TitleWord;

					break;
				}
			}
			return platm;
		}


		/* C  T I T L E  W O R D S  I N  P L A T M */
		/*----------------------------------------------------------------------------
		    %%Function: CTitleWordsInPlatm
		    %%Qualified: SList.SListApp.CTitleWordsInPlatm
		    %%Contact: rlittle

	    ----------------------------------------------------------------------------*/
		int CTitleWordsInPlatm(ArrayList platm)
		{
			int c = 0;

			foreach (ATM atm in platm)
			{
				if (atm.Atmt == ATMT.TitleWord)
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
		public int NMatch(ATMC atmc)
		{
			int i1 = this.Count - 1, i2 = atmc.Count - 1;
			int cMatch = 0;

			if (i1 < 0 || i2 < 0)
				return cMatch;

			while (i1 >= 0 && i2 >= 0)
			{
				// find the first titleword
				while (i2 >= 0 && atmc[i2].Atmt != ATMT.TitleWord)
					i2--;

				if (i2 < 0)
					break;

				while (i1 >= 0 && this[i1].Atmt != ATMT.TitleWord)
					i1--;

				if (i1 < 0)
					break;

				if (!this[i1].FMatch(atmc[i2]))
					break;

				cMatch++;
				i1--;
				i2--;
			}

			return (cMatch * 100) / this.CountWords;
		}
	}
}