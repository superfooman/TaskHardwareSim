using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskHardwareSim
{
    public partial class TaskHardware : Form
    {
        private BlockingCollection<string> finalResults;

        public TaskHardware()
        {
            InitializeComponent();
            finalResults = new BlockingCollection<string>();
        }

        private void buttonConnectDevice_Click(object sender, EventArgs e)
        {
            try
            {
                using (WaitingDialog frm = new WaitingDialog(CheckDeviceConnection, DeviceChoiceEnum.PS3))
                {
                    frm.ShowDialog();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                radioButtonSingleSiteEnable.Checked = false;
            }
        }

        private void CheckDeviceConnection(object deviceChoiceEnum)
        {
            try
            {
                int goodDeviceCount = HWDLL.CheckHardwareDevice(deviceChoiceEnum);
                MessageBox.Show("Good device count: " + goodDeviceCount);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void buttonProgram_Click(object sender, EventArgs e)
        {
            try
            {
                Stopwatch time = new Stopwatch();
                time.Start();

                using (WaitingDialog frm = new WaitingDialog(ProgramDevice, DeviceChoiceEnum.PS3))
                {
                    frm.ShowDialog();
                }
                updateTextBox();
                time.Stop();
                MessageBox.Show("Total time for programming the devices: " + time.ElapsedMilliseconds.ToString() + "ms");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                radioButtonSingleSiteEnable.Checked = false;
            }

        }

        private void updateTextBox()
        {
            TextBox[] textBoxes = new TextBox[] { textBox1, textBox2, textBox3, textBox4 };
            int total = finalResults.Count;
            for (int i = 0; i < total; i++)
            {
                textBoxes[i].Text = finalResults.Take();
            }

        }

        private void ProgramDevice(object deviceChoiceEnum)
        {
            try
            {
                HWDLL.CheckHardwareDevice(deviceChoiceEnum);

                List<Task> taskStart = new List<Task>();
                int i = 0;
                foreach (HWDLL device in HWDLL.Devices)
                {
                    object taskObject = device;
                    taskStart.Add(Task.Factory.StartNew(ProgramDeviceTask, taskObject));

                    if (radioButtonSingleSiteEnable.Checked)
                        taskStart[i].Wait();

                    i++;
                }

                if (!radioButtonSingleSiteEnable.Checked)
                {
                    Task.WaitAll(taskStart.ToArray());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void ProgramDeviceTask(object deviceObj)
        {
            try
            {
                string statuss_result = "";
                int status = HWDLL.InstallHardwareDevice(deviceObj);
                if (status == 0)
                {
                    statuss_result = "Successfully programmed " + deviceObj.ToString();
                    finalResults.Add(statuss_result);
                }
                else
                {
                    finalResults.Add("Failure status: " + status.ToString() + ". Device: " + deviceObj.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }
}
