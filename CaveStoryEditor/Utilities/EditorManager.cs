﻿using System;
using System.Collections.Generic;
using System.IO;
using CaveStoryModdingFramework.Stages;
using CaveStoryModdingFramework;
using System.Windows.Forms;

namespace CaveStoryEditor
{
    public class EditorManager
    {
        readonly Mod parentMod;
        readonly SpriteCache cache;
        
        readonly List<FormStageEditor> TileEditors;
        readonly List<FormScriptEditor> ScriptEditors;
        readonly List<FormAttributeEditor> AttributeEditors;
        
        public bool UnsavedChanges
        {
            get
            {
                foreach(var editor in TileEditors)
                {
                    if (editor.UnsavedEdits)
                        return true;
                }
                foreach (var editor in ScriptEditors)
                {
                    if (editor.UnsavedChanges)
                        return true;
                }
                foreach (var editor in AttributeEditors)
                {
                    if (editor.UnsavedEdits)
                        return true;
                }
                return false;
            }
        }

        public EditorManager(Mod parent, SpriteCache c)
        {
            parentMod = parent;
            cache = c;

            TileEditors = new List<FormStageEditor>();
            ScriptEditors = new List<FormScriptEditor>();
            AttributeEditors = new List<FormAttributeEditor>();
        }

        void RemoveEditor(object sender, EventArgs e)
        {
            if(sender is FormStageEditor fte)
            {
                TileEditors.Remove(fte);
            }
            else if(sender is FormScriptEditor fse)
            {
                ScriptEditors.Remove(fse);
            }
            else if(sender is FormAttributeEditor fae)
            {
                AttributeEditors.Remove(fae);
            }

            if (sender is IDisposable d)
                d.Dispose();
        }

        /// <summary>
        /// Closes every open editor
        /// </summary>
        public void Clear()
        {
            void clearList<T>(List<T> list) where T : Form
            {
                for (int i = 0; i < list.Count; i++)
                    //TODO fix the part where this doesn't *force* close the editor
                    list[i].Close();
            }
            clearList(TileEditors);
            clearList(ScriptEditors);
            clearList(AttributeEditors);
        }

        public FormStageEditor OpenTileEditor(StageEntry entry)
        {
            FormStageEditor editor = TileEditors.Find(x => x.stageEntry == entry);
            //if not, create it
            if (editor == null)
            {
                //HACK(?) todictionary()
                editor = new FormStageEditor(parentMod, cache, Keybinds.Default.StageEditor.ToDictionary(), entry);
                editor.FormClosed += RemoveEditor;
                TileEditors.Add(editor);
            }
            editor.Show();
            editor.Focus();
            return editor;
        }

        public void OpenScriptEditor(StageEntry entry)
        {
            FormScriptEditor editor = ScriptEditors.Find(x => x.stageEntry == entry);
            //if not, create it
            if (editor == null)
            {
                editor = new FormScriptEditor(parentMod, entry);
                editor.FormClosed += RemoveEditor;
                editor.ScriptSaved += Editor_ScriptSaved;
                ScriptEditors.Add(editor);
            }
            editor.Show();
            editor.Focus();
        }
        public void OpenScriptEditor(string path, bool encrypted, bool useScriptSource)
        {
            FormScriptEditor editor = ScriptEditors.Find(x => x.Fullpath == path);
            //if not, create it
            if (editor == null)
            {
                editor = new FormScriptEditor(parentMod, path, encrypted, useScriptSource);
                editor.FormClosed += RemoveEditor;
                editor.ScriptSaved += Editor_ScriptSaved;
                ScriptEditors.Add(editor);
            }
            editor.Show();
            editor.Focus();
        }
        public void OpenAttributeFile(string path)
        {
            FormAttributeEditor editor = AttributeEditors.Find(x => x.AttributeFilename == path);
            if (editor == null)
            {
                editor = new FormAttributeEditor(parentMod, path, Editor.Default.TileTypePath, Keybinds.Default.StageEditor.ToDictionary());
                editor.FormClosed += RemoveEditor;
                AttributeEditors.Add(editor);
            }
            editor.Show();
            editor.Focus();
        }

        private void Editor_ScriptSaved(object sender, EventArgs e)
        {
            /*
            var file = ((FormScriptEditor)sender).Fullpath;
            foreach(var editor in TileEditors)
            {
                if(editor.Filename == file)
                {
                    editor.NotifyMapStateRefreshNeeded();
                }
            }
            */
        }

        public void onScriptChanged(object sender, FileSystemEventArgs e)
        {
            /*
            //tell the tile editors to refresh their map states (or maybe put up a prompt?
            //tell the script editors to refresh their script
            foreach(var editor in TileEditors)
            {
                
            }
            */
        }
    }
}
