﻿using PortraitBuilder.Model.Portrait;
using System;
using System.Windows.Forms;

namespace PortraitBuilder.UI
{
    public partial class ImportDialog : Form
    {
        private bool isDNAValid = false;
        private bool isPropertiesValid = false;

        public Character character = new Character();

        public ImportDialog()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            character.Import(tbDNA.Text, tbProperties.Text);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void tb_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;

            if (tb == tbDNA)
            {
                isDNAValid = Validate(tb, DefaultCharacteristics.DNA.Length);
            }
            else if (tb == tbProperties)
            {
                isPropertiesValid = Validate(tb, DefaultCharacteristics.PROPERTIES.Length);
            }

            if (isDNAValid && isPropertiesValid)
            {
                btnOK.Enabled = true;
            }
            else
            {
                btnOK.Enabled = false;
            }
        }

        private bool Validate(TextBox tb, int length)
        {
            bool isValid = IsValid(tb.Text, length);
            if (isValid)
            {
                errorProvider.SetError(tb, string.Empty);
            }
            else
            {
                errorProvider.SetError(tb, "Invalid text.");
            }
            return isValid;
        }

        private bool IsValid(string dnaOrProperties, int length)
        {
            bool valid = true;

            foreach (char c in dnaOrProperties)
            {
                if (!Char.IsLetterOrDigit(c))
                {
                    valid = false;
                    break;
                }
            }

            if (dnaOrProperties.Length != length)
                valid = false;

            return valid;
        }
    }
}
