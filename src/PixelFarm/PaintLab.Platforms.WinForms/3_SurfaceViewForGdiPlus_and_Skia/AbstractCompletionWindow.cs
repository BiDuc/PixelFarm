﻿//MIT
//Mike Kruger, ICSharpCode,


using System;
using System.Windows.Forms;
namespace LayoutFarm.UI
{
    partial class AbstractCompletionWindow : Form
    {
        Form _linkedParentForm;
        Control _linkedParentControl;
        FormPopupShadow _formPopupShadow;

        FormClosingEventHandler _parentFormClosingEventHandler;
        EventHandler _parentFormSizeChanged;

        public AbstractCompletionWindow()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;

            _parentFormClosingEventHandler = (s, e) =>
            {
                //when parent form is closing 
                //we close the popup shadow window
                //and close this abstract completion windows

                if (_formPopupShadow != null)
                {
                    _formPopupShadow.Close();
                    _formPopupShadow = null;
                }
                //
                this.Close();
            };

            _parentFormSizeChanged = (s, e) =>
            {
#if DEBUG
                this.Hide();
#else
                this.Hide();
#endif
            };
        }
        internal FormPopupShadow PopupShadow
        {
            get { return _formPopupShadow; }
            set
            {
                _formPopupShadow = value;
            }
        }
        protected override void OnVisibleChanged(EventArgs e)
        {
            if (this.Visible)
            {
                //formPopupShadow.Show2(this);
            }
            else
            {
                //hide popup window
                _showingPopup = false;

                //hide popup shadow too
                _formPopupShadow.Visible = false;
            }
        }

        public Form LinkedParentForm
        {
            get { return _linkedParentForm; }
            set
            {
                if (_linkedParentForm != null && _linkedParentForm != value)
                {
                    //unsubscribe old event
                    _linkedParentForm.FormClosing -= _parentFormClosingEventHandler;
                    _linkedParentForm.Deactivate -= _parentFormSizeChanged;
                }

                _linkedParentForm = value;
                if (value != null)
                {
                    //when
                    _linkedParentForm.FormClosing += _parentFormClosingEventHandler;
                    _linkedParentForm.Deactivate += _parentFormSizeChanged;
                }
            }
        }



        public Control LinkedParentControl
        {
            get { return _linkedParentControl; }
            set
            {
                _linkedParentControl = value;
            }
        }

        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
        }

        bool _showingPopup = false;
        public void ShowForm()
        {


            // Show the window without activating it (i.e. do not take focus)
            PI.ShowWindow(this.Handle, (short)PI.SW_SHOWNOACTIVATE);
            if (_formPopupShadow != null)
            {
                _showingPopup = true;
                _formPopupShadow.Show2(this);
            }

            //this.Show();
            _linkedParentControl.Focus();
        }
        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);
            if (_showingPopup)
            {
                _formPopupShadow.MoveRelativeTo(this);
            }
        }
        public bool HasPopupShadow => _formPopupShadow != null;
         
    }
}
