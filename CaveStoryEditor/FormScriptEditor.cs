﻿using CaveStoryModdingFramework;
using CaveStoryModdingFramework.TSC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CaveStoryEditor
{
    public partial class FormScriptEditor : Form
    {
        public bool UnsavedChanges { get; private set; } = false;
        readonly Mod parentMod;
        public bool IsInStageFolder
        {
            get
            {
                //HACK not the most efficient way to check if a file is in a folder...
                var folder = Path.Combine(parentMod.DataFolderPath, parentMod.StageFolderPath);
                var s = Fullpath;
                do
                {
                    if (folder == (s = Path.GetDirectoryName(s)))
                    {
                        return true;
                    }
                } while (s.Length > 0);
                return false;
            }
        }
        public string Fullpath { get; private set; }
        public FormScriptEditor(Mod m, string path)
        {
            parentMod = m;
            Fullpath = path;

            InitializeComponent();

            if (Path.GetFileNameWithoutExtension(Fullpath).Contains("Credit"))
                ParserMode = ParserModes.Credits;
            else
                ParserMode = ParserModes.Default;

            Text = Path.GetFileName(Fullpath);

            if (File.Exists(path))
            {
                byte[] input = File.ReadAllBytes(Fullpath);
                if (parentMod.TSCEncrypted)
                    Encryptor.DecryptInPlace(input, parentMod.DefaultKey);
                mainScintilla.Text = parentMod.TSCEncoding.GetString(input);
            }
        }

        public event EventHandler ScriptSaved;

        void Save(string path)
        {
            var bytes = parentMod.TSCEncoding.GetBytes(mainScintilla.Text);
            if (parentMod.TSCEncrypted)
                Encryptor.EncryptInPlace(bytes);
            File.WriteAllBytes(path, bytes);
            
            UnsavedChanges = false;

            ScriptSaved?.Invoke(this, new EventArgs());
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save(Fullpath);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog()
            {
                Filter = string.Join("|", $"TSC Files (*.{parentMod.TSCExtension})|*.{parentMod.TSCExtension}", FormMain.AllFilesFilter)
            })
            {
                if(sfd.ShowDialog() == DialogResult.OK)
                {
                    Save(sfd.FileName);
                }
            }            
        }

        private void mainScintilla_TextChanged(object sender, EventArgs e)
        {
            UnsavedChanges = true;
        }

        enum ParserModes
        {
            None,
            Default,
            Credits,
            Scriptsource
        }
        ParserModes parserMode = ParserModes.None;
        ParserModes ParserMode
        {
            get => parserMode;
            set
            {
                if (parserMode != value)
                {
                    parserMode = value;
                    defaultToolStripMenuItem.Checked = ParserMode == ParserModes.Default;
                    creditsToolStripMenuItem.Checked = ParserMode == ParserModes.Credits;
                    InitParserStyles(ParserMode, mainScintilla);
                }
            }
        }

        private void InitParserStyles(ParserModes mode, ScintillaNET.Scintilla scintilla)
        {
            scintilla.ClearDocumentStyle();
            switch (mode)
            {
                case ParserModes.Default:
                case ParserModes.Scriptsource:
                    break;
                case ParserModes.Credits:
                    for (int i = 0; i <= 10; i++)
                        scintilla.Styles[i].Font = "Consolas";

                    scintilla.Styles[StyleValue].ForeColor = Color.FromArgb(0xC42F63);

                    scintilla.Styles[StyleSeparater].ForeColor = Color.Gray;

                    scintilla.Styles[StyleLabel].ForeColor = Color.Black;
                    scintilla.Styles[StyleLabel].Bold = true;

                    scintilla.Styles[StyleJump].ForeColor = Color.Blue;
                    //scintilla.Styles[StyleJump].Bold = true;

                    scintilla.Styles[StyleOffset].ForeColor = Color.Blue;
                    //scintilla.Styles[StyleOffset].Bold = true;

                    scintilla.Styles[StyleBrackets].ForeColor = Color.Blue;

                    scintilla.Styles[StyleFade].ForeColor = Color.Blue;
                    //scintilla.Styles[StyleFade].Bold = true;

                    scintilla.Styles[StyleEnd].ForeColor = Color.Blue;
                    //scintilla.Styles[StyleEnd].Bold = true;

                    scintilla.Styles[StyleMusic].ForeColor = Color.Blue;
                    //scintilla.Styles[StyleMusic].Bold = true;

                    scintilla.Styles[StyleComment].ForeColor = Color.FromArgb(0x367A2A);
                    scintilla.Styles[StyleComment].Italic = true;
                    break;
            }
            scintilla.StartStyling(0);
        }

        private void mainScintilla_StyleNeeded(object sender, ScintillaNET.StyleNeededEventArgs e)
        {
            var s = (ScintillaNET.Scintilla)sender;
            switch(ParserMode)
            {
                case ParserModes.Scriptsource:
                case ParserModes.Default:
                    StyleTSC(s, s.GetEndStyled(), e.Position);
                    break;
                case ParserModes.Credits:
                    StyleCredits(s, s.GetEndStyled(), e.Position);
                    break;
            }            
        }

        private void StyleTSC(ScintillaNET.Scintilla scintilla, int startPos, int endPos)
        {

        }

        const int StyleValue = 1;
        const int StyleSeparater = 2;
        const int StyleLabel = 3;
        const int StyleJump = 4;
        const int StyleOffset = 5;
        const int StyleBrackets = 6;
        const int StyleFade = 7;
        const int StyleEnd = 8;
        const int StyleMusic = 9;
        const int StyleComment = 10;
        
        private void StyleCredits(ScintillaNET.Scintilla scintilla, int startPos, int endPos)
        {
            bool IsValidArg(int offset)
            {
                return char.IsDigit((char)scintilla.GetCharAt(offset))
                            && char.IsDigit((char)scintilla.GetCharAt(offset + 1))
                            && char.IsDigit((char)scintilla.GetCharAt(offset + 2))
                            && char.IsDigit((char)scintilla.GetCharAt(offset + 3));
            }
            
            //scan backwards for the first [, or the start of the file
            if(startPos > 0)
                startPos--;
            while (startPos > 0 && (char)scintilla.GetCharAt(startPos) != '[')
                startPos--;
            
            scintilla.StartStyling(startPos);
            while(startPos < endPos)
            {
                switch((char)scintilla.GetCharAt(startPos))
                {
                    case '[':
                        int textLen = 0;
                        char c;
                        //scan for the matching brace, or the end of the doc
                        for (c = (char)scintilla.GetCharAt(++startPos);
                            startPos < scintilla.TextLength && c != ']';
                            c = (char)scintilla.GetCharAt(++startPos))
                        {
                            textLen++;
                        }
                        //style the inner text if we found a brace
                        if (c == ']')
                        {
                            scintilla.SetStyling(1, StyleBrackets);
                            scintilla.SetStyling(textLen, 0);
                            goto case ']';
                        }
                        //otherwise it's just a comment I guess?
                        else
                        {
                            startPos -= textLen;
                            goto default;
                        }
                        break;
                    case ']':
                        scintilla.SetStyling(1, StyleBrackets);
                        scintilla.SetStyling(4, StyleValue);
                        startPos += 5;
                        break;

                    case 'l':
                        //char prevChar = (char)scintilla.GetCharAt(startPos - 1);
                        if (IsValidArg(startPos + 1))
                        {
                            scintilla.SetStyling(1, StyleLabel);
                            //scintilla.SetStyling(4, StyleValue);
                            scintilla.SetStyling(4, StyleLabel);
                            startPos += 5;
                        }
                        else
                            goto default;
                        break;

                    case 'f':
                        scintilla.SetStyling(1, StyleJump);
                        scintilla.SetStyling(4, StyleValue);
                        scintilla.SetStyling(1, StyleSeparater);
                        scintilla.SetStyling(4, StyleValue);
                        startPos += 10;
                        break;
                    case 'j':
                        scintilla.SetStyling(1, StyleJump);
                        scintilla.SetStyling(4, StyleValue);
                        startPos += 5;
                        break;

                    case '-':
                    case '+':
                        scintilla.SetStyling(1, StyleOffset);
                        scintilla.SetStyling(4, StyleValue);
                        startPos += 5;
                        break;

                    case '!':
                        scintilla.SetStyling(1, StyleMusic);
                        scintilla.SetStyling(4, StyleValue);
                        startPos += 5;
                        break;
                    case '~':
                        scintilla.SetStyling(1, StyleFade);
                        startPos++;
                        break;
                    case '/':
                        scintilla.SetStyling(1, StyleEnd);
                        startPos++;
                        break;
                    default:
                        scintilla.SetStyling(1, StyleComment);
                        startPos++;
                        break;
                }
            }
        }

        private void defaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ParserMode = ParserModes.Default;
        }

        private void creditsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ParserMode = ParserModes.Credits;
        }
    }
}
