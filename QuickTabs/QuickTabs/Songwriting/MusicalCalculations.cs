using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Songwriting
{
    public class Chord
    {
        public Note RootNote;
        public ChordType ChordType;
        public XTh Th = XTh.Unset;
        public List<int> Added = new List<int>();
        public int Suspended = -1;
        public bool Diminished = false;
        public List<int> Sharped = new List<int>();
        public List<int> Flatted = new List<int>();

        public Chord() { }
        public Chord(string Chord)
        {
            Chord c = Parse(Chord);
            RootNote = c.RootNote;
            ChordType = c.ChordType;
            Th = c.Th;
            Added = c.Added;
            Suspended = c.Suspended;
            Diminished = c.Diminished;
            Sharped = c.Sharped;
            Flatted = c.Flatted;
        }
        public static Chord CSharpMinor
        {
            get
            {
                return Chord.Parse("C#m");
            }
        }
        public static Chord Parse(string ChordS)
        {
            Chord c = new Chord();
            ChordS += "end ";
            ChordS = ChordS.ToLower();
            bool rootNoteSet = false;
            int rootNoteLength;
            for (rootNoteLength = 1; true; rootNoteLength++)
            {
                try
                {
                    c.RootNote = Note.Parse(ChordS.Substring(0, rootNoteLength));
                    rootNoteSet = true;
                } catch (Exception ex) {
                    break;
                }
            }
            if (!rootNoteSet)
            {
                throw new FormatException("Invalid root note.");
            }
            bool chordtypeset = false;
            bool thset = false;
            bool suspendedset = false;
            bool diminishedset = false;
            c.Diminished = false;
            ChordS = ChordS.Substring(rootNoteLength - 1);
            StringParser parser = new StringParser(ChordS);
            while (true)
            {
                string usedSep;
                string taken = parser.TakeUntil(out usedSep, new[] { "end", "maj", "m", "add", "dim", "sus", "#", "b", "♭", "6/9" });

                if (taken.Length > 0)
                {
                    int result;
                    if (int.TryParse(taken.Replace("/", ""), out result))
                    {
                        if (thset)
                        {
                            throw new FormatException("Chord was set to the " + c.Th.thType.ToString() + " " + c.Th.number + "th, you can't express the Xth twice.");
                        }
                        thset = true;
                        c.Th = new XTh(result, ThType.Dominant);
                    }
                    else if (taken != "/")
                    {
                        throw new FormatException("Unknown musical term \"" + taken + "\".");
                    }
                }
                if (usedSep == "end")
                {
                    return c;
                }
                if (usedSep == "6/9")
                {
                    throw new FormatException("Instead of \"6/9\", use \"6add9\".");
                }
                if (usedSep == "m")
                {
                    if (chordtypeset)
                    {
                        throw new FormatException("Chord was already set as minor");
                    }
                    c.ChordType = ChordType.Minor;
                    chordtypeset = true;
                }
                if (usedSep == "maj")
                {
                    if (thset)
                    {
                        throw new FormatException("Chord was set to the " + c.Th.thType.ToString() + " " + c.Th.number + "th earlier, you can't express the Xth twice.");
                    }
                    int val = -1;
                    for (int i = 1; true; i++)
                    {
                        //Console.WriteLine(parser.Peek(i));
                        int oldval = val;
                        if (!int.TryParse(parser.Peek(i), NumberStyles.None, new CultureInfo("en-US"), out val))
                        {
                            val = oldval;
                            break;
                        }
                    }
                    if (val == -1)
                    {
                        throw new FormatException("No value was given after \"maj\" or value given was invalid.");
                    }
                    thset = true;
                    c.Th = new XTh(val, ThType.Major);
                    parser.Seek(val.ToString().Length, System.IO.SeekOrigin.Current);
                }
                if (usedSep == "add")
                {
                    int val = -1;
                    for (int i = 1; true; i++)
                    {
                        //Console.WriteLine(parser.Peek(i));
                        int oldval = val;
                        if (!int.TryParse(parser.Peek(i), NumberStyles.None, new CultureInfo("en-US"), out val))
                        {
                            val = oldval;
                            break;
                        }
                    }
                    if (val == -1)
                    {
                        throw new FormatException("No value was given for what to add or the value was invalid.");
                    }
                    c.Added.Add(val);
                    parser.Seek(val.ToString().Length, System.IO.SeekOrigin.Current);
                }
                if (usedSep == "dim")
                {
                    if (diminishedset)
                    {
                        throw new FormatException("Chord was already set as diminished. If you are trying to express a half diminished chord, try \"mb5\". A half diminished 7th would be \"m7b5\"");
                    }
                    diminishedset = true;
                    c.Diminished = true;
                }
                if (usedSep == "sus")
                {
                    if (suspendedset)
                    {
                        throw new FormatException("Chord was already set as \"sus" + c.Suspended + "\". You cannot suspend a chord twice.");
                    }
                    int val = -1;
                    for (int i = 1; true; i++)
                    {
                        //Console.WriteLine(parser.Peek(i));
                        int oldval = val;
                        if (!int.TryParse(parser.Peek(i), NumberStyles.None, new CultureInfo("en-US"), out val))
                        {
                            val = oldval;
                            break;
                        }
                    }
                    if (val == -1)
                    {
                        throw new FormatException();
                    }
                    suspendedset = true;
                    c.Suspended = val;
                    parser.Seek(val.ToString().Length, System.IO.SeekOrigin.Current);
                }
                if (usedSep == "#")
                {
                    int val = -1;
                    for (int i = 1; true; i++)
                    {
                        //Console.WriteLine(parser.Peek(i));
                        int oldval = val;
                        if (!int.TryParse(parser.Peek(i), NumberStyles.None, new CultureInfo("en-US"), out val))
                        {
                            val = oldval;
                            break;
                        }
                    }
                    if (val == -1)
                    {
                        throw new FormatException("No value was given for what to sharp or the value was invalid.");
                    }
                    if (val == 1)
                    {
                        throw new FormatException("You cannot sharp the root note later in the chord. Just make your root note sharp.");
                    }
                    c.Sharped.Add(val);
                    parser.Seek(val.ToString().Length, System.IO.SeekOrigin.Current);
                }
                if (usedSep == "b" || usedSep == "♭")
                {
                    int val = -1;
                    for (int i = 1; true; i++)
                    {
                        //Console.WriteLine(parser.Peek(i));
                        int oldval = val;
                        if (!int.TryParse(parser.Peek(i), NumberStyles.None, new CultureInfo("en-US"), out val))
                        {
                            val = oldval;
                            break;
                        }
                    }
                    if (val == -1)
                    {
                        throw new FormatException("No value was given for what to flat or the value given was invalid.");
                    }
                    if (val == 1)
                    {
                        throw new FormatException("You cannot flat the root note later in the chord. Just make your root note flat.");
                    }
                    c.Flatted.Add(val);
                    parser.Seek(val.ToString().Length, System.IO.SeekOrigin.Current);
                }
            }
        }
        public struct formulaNumber
        {
            public int Number;
            public int SemitoneOffset;
            public formulaNumber(int Number, int SemitoneOffset)
            {
                this.Number = Number;
                this.SemitoneOffset = SemitoneOffset;
            }
        }
        public List<formulaNumber> formula
        {
            get
            {
                List<formulaNumber> formula = new List<formulaNumber>();
                formula.Add(new formulaNumber(1, 0));
                if (Suspended == -1)
                {
                    if (ChordType == ChordType.Major)
                    {
                        formula.Add(new formulaNumber(3, 0));
                    } else if (ChordType == ChordType.Minor)
                    {
                        formula.Add(new formulaNumber(3, -1));
                    }
                    formula.Add(new formulaNumber(5, 0));
                } else
                {
                    if (ChordType == ChordType.Major)
                    {
                        formula.Add(new formulaNumber(Suspended, 0));
                    }
                    else if (ChordType == ChordType.Minor)
                    {
                        formula.Add(new formulaNumber(Suspended, -1));
                    }
                    formula.Add(new formulaNumber(5, 0));
                }
                if (Th.number != XTh.Unset.number)
                {
                    if (Th.number == 6)
                    {
                        formula.Add(new formulaNumber(6, 0));
                    } else
                    {
                        for (int i = 7; i <= Th.number; i += 2)
                        {
                            if (i == 7 && Th.thType == ThType.Dominant)
                            {
                                formula.Add(new formulaNumber(7, -1));
                            } else
                            {
                                formula.Add(new formulaNumber(i, 0));
                            }
                        }
                    }
                }
                foreach (int i in Added)
                {
                    formula.Add(new formulaNumber(i, 0));
                }
                foreach (int sharp in Sharped)
                {
                    bool sharped = false;
                    for (int formi = 0; formi < formula.Count; formi++)
                    {
                        if (formula[formi].Number == sharp)
                        {
                            formula[formi] = new formulaNumber(formula[formi].Number, formula[formi].SemitoneOffset + 1);
                            sharped = true;
                        }
                    }
                    if (!sharped)
                    {
                        formula.Add(new formulaNumber(sharp, 1));
                    }
                }
                foreach (int flat in Flatted)
                {
                    bool flatted = false;
                    for (int formi = 0; formi < formula.Count; formi++)
                    {
                        if (formula[formi].Number == flat)
                        {
                            formula[formi] = new formulaNumber(formula[formi].Number, formula[formi].SemitoneOffset - 1);
                            flatted = true;
                        }
                    }
                    if (!flatted)
                    {
                        formula.Add(new formulaNumber(flat, -1));
                    }
                }
                if (Diminished)
                {
                    for (int i = 1; i < formula.Count; i++)
                    {
                        formula[i] = new formulaNumber(formula[i].Number, formula[i].SemitoneOffset - 1);
                    }
                }
                return formula;
            }    
        }
        public List<Note> ToNotes()
        {
            List<Note> notes = new List<Note>();
            foreach (formulaNumber n in formula)
            {
                Note octified = Note.GetMajorScaleNote(RootNote, n.Number);
                //Console.WriteLine(n.Number + " " + Note.ToSemitones(RootNote, octified) + " " + n.SemitoneOffset);
                notes.Add(Note.FromSemitones(RootNote, Note.ToSemitones(RootNote, octified) + n.SemitoneOffset));
            }
            /*foreach (formulaNumber n in formula)
            {
                int semitones = (n.Number - 1) * 2;
                semitones += n.SemitoneOffset;
                notes.Add(Note.FromSemitones(RootNote, semitones));
            }*/
            return notes;
        }
        public struct XTh
        {
            internal int number;
            internal ThType thType;
            public XTh(int Number, ThType ThType)
            {
                thType = ThType;
                if (Number == 6 || (Number >= 7 && Number % 2 > 0))
                {
                    number = Number;
                }
                else
                {
                    throw new FormatException("XTh number must be 6 or an odd number 7 or larger.");
                }
            }
            internal static XTh Unset = new XTh() { number = -1, thType = ThType.Dominant };
        }
    }
    public struct Note
    {
        public NoteValue NoteValue;
        public NoteType NoteType;
        public int Octave;
        public Note(NoteValue Note, NoteType NoteType, int RelativeOctave)
        {
            if ((Note == NoteValue.B || Note == NoteValue.E) && NoteType == NoteType.Sharp)
            {
                if (Note == NoteValue.B)
                {
                    Note = NoteValue.C;
                    NoteType = NoteType.Natural;
                } else
                {
                    Note = NoteValue.F;
                    NoteType = NoteType.Natural;
                }
            } else if ((Note == NoteValue.C || Note == NoteValue.F) && NoteType == NoteType.Flat)
            {
                if (Note == NoteValue.C)
                {
                    Note = NoteValue.B;
                    NoteType = NoteType.Natural;
                }
                else
                {
                    Note = NoteValue.E;
                    NoteType = NoteType.Natural;
                }
            }
            NoteValue = Note;
            this.NoteType = NoteType;
            Octave = RelativeOctave;
        }
        public Note(string Note)
        {
            Note n = Parse(Note);
            NoteValue = n.NoteValue;
            NoteType = n.NoteType;
            Octave = n.Octave;
        }
        private const int c0MidiNumber = 12;
        public int MidiNumber
        {
            get
            {
                int octaveStart = c0MidiNumber * (Octave + 1);
                return octaveStart + Note.ToSemitones(new Note(NoteValue.C, NoteType.Natural, Octave), this);
            }
        }
        public static Note GetMajorScaleNote(Note Scale, int Note)
        {
            int[] intervals = new[] { 0, 2, 4, 5, 7, 9, 11, 12 };
            Note currentNote = Scale;
            Note--;
            int semitones = 0;
            while (true)
            {
                if (Note < intervals.Length)
                {
                    semitones += intervals[Note];
                    return QuickTabs.Songwriting.Note.FromSemitones(Scale, semitones);
                } else
                {
                    semitones += intervals[intervals.Length - 1];
                    Note -= intervals.Length;
                }
            }
        }
        public static int ToSemitones(Note Scale, Note Note)
        {
            Note n = Scale;
            int i = 0;
            for (i = 0; !(n.NoteType == Note.NoteType && n.NoteValue == Note.NoteValue); i++)
            {
                if ((n.NoteValue == NoteValue.E || n.NoteValue == NoteValue.B) && n.NoteType == NoteType.Natural)
                {
                    if (n.NoteValue == NoteValue.E)
                    {
                        n.NoteValue = NoteValue.F;
                    }
                    else
                    {
                        n.NoteValue = NoteValue.C;
                    }
                }
                else
                {
                    if (n.NoteType == NoteType.Flat)
                    {
                        n.NoteType = NoteType.Natural;
                    }
                    else if (n.NoteType == NoteType.Sharp)
                    {
                        if (n.NoteValue == NoteValue.G)
                        {
                            n.NoteValue = NoteValue.A;
                        }
                        else
                        {
                            n.NoteValue = (NoteValue)((byte)n.NoteValue + 1);
                        }
                        n.NoteType = NoteType.Natural;
                    }
                    else if (n.NoteType == NoteType.Natural)
                    {
                        n.NoteType = NoteType.Sharp;
                    }
                }
                if (n.NoteValue == NoteValue.C && n.NoteType == NoteType.Natural)
                {
                    n.Octave++;
                }
            }
            return i;
        }
        public static Note FromSemitones(Note Scale, int Semitones)
        {
            Note n = new Note(Scale.NoteValue, Scale.NoteType, Scale.Octave);
            for (int i = 0; i < Semitones; i++)
            {
                //Console.Write(n.ToString() + " - ");
                if ((n.NoteValue == NoteValue.E || n.NoteValue == NoteValue.B) && n.NoteType == NoteType.Natural)
                {
                    if (n.NoteValue == NoteValue.E)
                    {
                        n.NoteValue = NoteValue.F;
                    } else
                    {
                        n.NoteValue = NoteValue.C;
                    }
                } else
                {
                    if (n.NoteType == NoteType.Flat)
                    {
                        n.NoteType = NoteType.Natural;
                    } else if (n.NoteType == NoteType.Sharp)
                    {
                        if (n.NoteValue == NoteValue.G)
                        {
                            n.NoteValue = NoteValue.A;
                        } else
                        {
                            n.NoteValue = (NoteValue)((byte)n.NoteValue + 1);
                        }
                        n.NoteType = NoteType.Natural;
                    } else if (n.NoteType == NoteType.Natural)
                    {
                        n.NoteType = NoteType.Sharp;
                    }
                }
                if (n.NoteValue == NoteValue.C && n.NoteType == NoteType.Natural)
                {
                    n.Octave++;
                }
                //Console.WriteLine(n.ToString());
            }
            //Console.WriteLine();
            return n;
        }
        public static List<Note> GetScaleNotes(Note Scale)
        {
            List<Note> scaleNotes = new List<Note>();
            Note cNote = new Note(Scale.NoteValue, Scale.NoteType, Scale.Octave);
            for (int i = 0; i <= 6; i++)
            {
                scaleNotes.Add(cNote);
                if (cNote.NoteValue == NoteValue.G)
                {
                    cNote.NoteValue = NoteValue.A;
                } else
                {
                    cNote.NoteValue = (NoteValue)((byte)cNote.NoteValue + 1);
                }
            }
            scaleNotes.Add(new Note(Scale.NoteValue, Scale.NoteType, Scale.Octave + 1));
            return scaleNotes;
        }
        public static Note Parse(string Note)
        {
            Note n = new Note();
            if (Note.Length == 0)
            {
                throw new FormatException();
            }
            NoteValue result;
            if (!Enum.TryParse<NoteValue>(Note[0].ToString().ToUpper(), out result))
            {
                throw new FormatException();
            }
            n.NoteValue = result;
            if (Note.Length > 1)
            {
                if (Note[1] == 'b' || Note[1] == '♭')
                {
                    n.NoteType = NoteType.Flat;
                    if (Note.Length > 2)
                    {
                        int iResult;
                        if (!int.TryParse(Note.Substring(2), out iResult))
                        {
                            throw new FormatException();
                        }
                        n.Octave = iResult;
                    }
                } else if (Note[1] == '#')
                {
                    n.NoteType = NoteType.Sharp;
                    if (Note.Length > 2)
                    {
                        int iResult;
                        if (!int.TryParse(Note.Substring(2), out iResult))
                        {
                            throw new FormatException();
                        }
                        n.Octave = iResult;
                    }
                } else
                {
                    int iResult;
                    if (!int.TryParse(Note.Substring(1), out iResult))
                    {
                        throw new FormatException();
                    }
                    n.Octave = iResult;
                }
            } else
            {
                n.NoteType = NoteType.Natural;
            }
            if ((n.NoteValue == NoteValue.B || n.NoteValue == NoteValue.E) && n.NoteType == NoteType.Sharp)
            {
                if (n.NoteValue == NoteValue.B)
                {
                    n.NoteValue = NoteValue.C;
                    n.NoteType = NoteType.Natural;
                }
                else
                {
                    n.NoteValue = NoteValue.F;
                    n.NoteType = NoteType.Natural;
                }
            }
            else if ((n.NoteValue == NoteValue.C || n.NoteValue == NoteValue.F) && n.NoteType == NoteType.Flat)
            {
                if (n.NoteValue == NoteValue.C)
                {
                    n.NoteValue = NoteValue.B;
                    n.NoteType = NoteType.Natural;
                }
                else
                {
                    n.NoteValue = NoteValue.E;
                    n.NoteType = NoteType.Natural;
                }
            }
            return n;
        }
        public override string ToString()
        {
            return ToString(true);
        }
        public string ToString(bool showOctave)
        {
            string str;
            if (NoteType == NoteType.Natural)
            {
                str = NoteValue.ToString();
            } else if (NoteType == NoteType.Flat)
            {
                str = NoteValue.ToString() + "b";
            } else
            {
                str = NoteValue.ToString() + "#";
            }
            if (showOctave)
            {
                str += Octave.ToString();
            }
            return str;
        }
    }
    public enum NoteValue : byte
    {
        A = 0,
        B = 1,
        C = 2,
        D = 3,
        E = 4,
        F = 5,
        G = 6
    }
    public enum NoteType : byte
    {
        Natural = 0,
        Flat = 1,
        Sharp = 2
    }
    public enum ChordType : byte
    {
        Major = 0,
        Minor = 1
    }
    public enum ThType : byte
    {
        Dominant = 0,
        Major = 1
    }
}
