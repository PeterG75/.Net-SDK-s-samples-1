' ReSharper disable InconsistentNaming

Imports System.IO
Imports VisioForge.Controls.UI
Imports VisioForge.Controls.UI.Dialogs.OutputFormats
Imports VisioForge.Controls.UI.Dialogs.VideoEffects
Imports VisioForge.Shared.IPCameraDB
Imports VisioForge.Types
Imports VisioForge.Tools
Imports VisioForge.Types.OutputFormat
Imports VisioForge.Types.Sources
Imports VisioForge.Types.VideoEffects

Public Class Form1
    Dim mp4v11SettingsDialog As MFSettingsDialog

    Dim mpegTSSettingsDialog As MFSettingsDialog

    Dim movSettingsDialog As MFSettingsDialog

    Dim mp4V10SettingsDialog As MP4v10SettingsDialog

    Dim aviSettingsDialog As AVISettingsDialog

    Dim wmvSettingsDialog As WMVSettingsDialog

    Dim gifSettingsDialog As GIFSettingsDialog

    Dim screenshotSaveDialog As SaveFileDialog

    Dim onvifControl As ONVIFControl

    Dim onvifPtzRanges As ONVIFPTZRanges

    Dim onvifPtzX As Double

    Dim onvifPtzY As Double

    Dim onvifPtzZoom As Double

    Private ReadOnly tmRecording As Timers.Timer = New Timers.Timer(1000)

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As EventArgs) Handles MyBase.Load
        Text += " (SDK v" + VideoCapture1.SDK_Version.ToString() + ", " + VideoCapture1.SDK_State + ")"

        AddHandler tmRecording.Elapsed, AddressOf UpdateRecordingTime

        screenshotSaveDialog = New SaveFileDialog()
        screenshotSaveDialog.FileName = "image.jpg"
        screenshotSaveDialog.Filter = "JPEG|*.jpg|BMP|*.bmp|PNG|*.png|GIF|*.gif|TIFF|*.tiff"
        screenshotSaveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\VisioForge\"

        cbIPCameraType.SelectedIndex = 2
        cbOutputFormat.SelectedIndex = 2

        edOutput.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\VisioForge\" + "output.mp4"
    End Sub

    Private Sub btSelectOutput_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btSelectOutput.Click
        If saveFileDialog1.ShowDialog() = DialogResult.OK Then
            edOutput.Text = saveFileDialog1.FileName
        End If
    End Sub

    Private Sub SetMP4v11Output(ByRef mp4Output As VFMP4v11Output)
        If (mp4v11SettingsDialog Is Nothing) Then
            mp4v11SettingsDialog = New MFSettingsDialog(MFSettingsDialogMode.MP4v11)
        End If

        mp4v11SettingsDialog.SaveSettings(mp4Output)
    End Sub

    Private Sub SetMPEGTSOutput(ByRef mpegTSOutput As VFMPEGTSOutput)

        If (mpegTSSettingsDialog Is Nothing) Then
            mpegTSSettingsDialog = New MFSettingsDialog(MFSettingsDialogMode.MPEGTS)
        End If

        mpegTSSettingsDialog.SaveSettings(mpegTSOutput)
    End Sub

    Private Sub SetMOVOutput(ByRef mkvOutput As VFMOVOutput)

        If (movSettingsDialog Is Nothing) Then
            movSettingsDialog = New MFSettingsDialog(MFSettingsDialogMode.MOV)
        End If

        movSettingsDialog.SaveSettings(mkvOutput)
    End Sub

    Private Sub SetMP4v10Output(ByRef mp4Output As VFMP4v8v10Output)
        If (mp4V10SettingsDialog Is Nothing) Then
            mp4V10SettingsDialog = New MP4v10SettingsDialog()
        End If

        mp4V10SettingsDialog.SaveSettings(mp4Output)
    End Sub

    Private Sub SetGIFOutput(ByRef gifOutput As VFAnimatedGIFOutput)
        If (gifSettingsDialog Is Nothing) Then
            gifSettingsDialog = New GIFSettingsDialog()
        End If

        gifSettingsDialog.SaveSettings(gifOutput)
    End Sub

    Private Sub SetWMVOutput(ByRef wmvOutput As VFWMVOutput)
        If (wmvSettingsDialog Is Nothing) Then
            wmvSettingsDialog = New WMVSettingsDialog(VideoCapture1)
        End If

        wmvSettingsDialog.WMA = False
        wmvSettingsDialog.SaveSettings(wmvOutput)
    End Sub

    Private Sub SetAVIOutput(ByRef aviOutput As VFAVIOutput)
        If (aviSettingsDialog Is Nothing) Then
            aviSettingsDialog = New AVISettingsDialog(
                VideoCapture1.Video_Codecs.ToArray(),
                VideoCapture1.Audio_Codecs.ToArray())
        End If

        aviSettingsDialog.SaveSettings(aviOutput)

        If (aviOutput.Audio_UseMP3Encoder) Then

            Dim mp3Output = New VFMP3Output()
            aviOutput.MP3 = mp3Output
        End If
    End Sub

    Private Async Sub btStart_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btStart.Click

        If (onvifControl IsNot Nothing) Then
            onvifControl.Disconnect()
            onvifControl.Dispose()
            onvifControl = Nothing

            btONVIFConnect.Text = "Connect"
        End If

        mmLog.Clear()

        VideoCapture1.Video_Sample_Grabber_Enabled = True

        VideoCapture1.Video_Renderer.Zoom_Ratio = 0
        VideoCapture1.Video_Renderer.Zoom_ShiftX = 0
        VideoCapture1.Video_Renderer.Zoom_ShiftY = 0

        VideoCapture1.Debug_Mode = cbDebugMode.Checked
        VideoCapture1.Debug_Dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\VisioForge\"

        VideoCapture1.Audio_RecordAudio = cbIPAudioCapture.Checked
        VideoCapture1.Audio_PlayAudio = cbIPAudioCapture.Checked

        VideoCapture1.Video_Renderer_SetAuto

        'source
        VideoCapture1.IP_Camera_Source = New IPCameraSourceSettings()

        Select Case (cbIPCameraType.SelectedIndex)

            Case 0 : VideoCapture1.IP_Camera_Source.Type = VFIPSource.Auto_VLC

            Case 1 : VideoCapture1.IP_Camera_Source.Type = VFIPSource.Auto_FFMPEG

            Case 2 : VideoCapture1.IP_Camera_Source.Type = VFIPSource.Auto_LAV

            Case 3 : VideoCapture1.IP_Camera_Source.Type = VFIPSource.RTSP_Live555

            Case 4 : VideoCapture1.IP_Camera_Source.Type = VFIPSource.HTTP_FFMPEG

            Case 5 : VideoCapture1.IP_Camera_Source.Type = VFIPSource.MMS_WMV

            Case 6 : VideoCapture1.IP_Camera_Source.Type = VFIPSource.RTSP_UDP_FFMPEG

            Case 7 : VideoCapture1.IP_Camera_Source.Type = VFIPSource.RTSP_TCP_FFMPEG

            Case 8 : VideoCapture1.IP_Camera_Source.Type = VFIPSource.RTSP_HTTP_FFMPEG

            Case 9
                VideoCapture1.IP_Camera_Source.Type = VFIPSource.HTTP_MJPEG_LowLatency
                cbIPAudioCapture.Checked = False
                VideoCapture1.Audio_RecordAudio = False
                VideoCapture1.Audio_PlayAudio = False

            Case 10
                VideoCapture1.IP_Camera_Source.Type = VFIPSource.RTSP_LowLatency
                VideoCapture1.IP_Camera_Source.RTSP_LowLatency_UseUDP = False
            Case 11
                VideoCapture1.IP_Camera_Source.Type = VFIPSource.RTSP_LowLatency
                VideoCapture1.IP_Camera_Source.RTSP_LowLatency_UseUDP = True
            Case 12
                VideoCapture1.IP_Camera_Source.Type = VFIPSource.NDI
            Case 13
                VideoCapture1.IP_Camera_Source.Type = VFIPSource.NDI_Legacy
        End Select

        VideoCapture1.IP_Camera_Source.URL = cbIPURL.Text
        VideoCapture1.IP_Camera_Source.AudioCapture = cbIPAudioCapture.Checked
        VideoCapture1.IP_Camera_Source.Login = edIPLogin.Text
        VideoCapture1.IP_Camera_Source.Password = edIPPassword.Text
        VideoCapture1.IP_Camera_Source.VLC_ZeroClockJitterEnabled = cbVLCZeroClockJitter.Checked
        VideoCapture1.IP_Camera_Source.VLC_CustomLatency = Convert.ToInt32(edVLCCacheSize.Text)

        If (cbIPCameraONVIF.Checked) Then
            VideoCapture1.IP_Camera_Source.ONVIF_Source = True

            If (cbONVIFProfile.SelectedIndex <> -1) Then
                VideoCapture1.IP_Camera_Source.ONVIF_SourceProfile = cbONVIFProfile.Text
            End If
        End If

        If rbPreview.Checked Then
            VideoCapture1.Mode = VFVideoCaptureMode.IPPreview
        Else
            VideoCapture1.Mode = VFVideoCaptureMode.IPCapture
            VideoCapture1.Output_Filename = edOutput.Text

            Select Case (cbOutputFormat.SelectedIndex)
                Case 0
                    Dim aviOutput = New VFAVIOutput()
                    SetAVIOutput(aviOutput)
                    VideoCapture1.Output_Format = aviOutput
                Case 1
                    Dim wmvOutput = New VFWMVOutput()
                    SetWMVOutput(wmvOutput)
                    VideoCapture1.Output_Format = wmvOutput
                Case 2
                    Dim mp4Output = New VFMP4v8v10Output()
                    SetMP4v10Output(mp4Output)
                    VideoCapture1.Output_Format = mp4Output
                Case 3
                    Dim mp4Output = New VFMP4v11Output()
                    SetMP4v11Output(mp4Output)
                    VideoCapture1.Output_Format = mp4Output
                Case 4
                    Dim gifOutput = New VFAnimatedGIFOutput()
                    SetGIFOutput(gifOutput)

                    VideoCapture1.Output_Format = gifOutput
                Case 5
                    Dim tsOutput = New VFMPEGTSOutput()
                    SetMPEGTSOutput(tsOutput)
                    VideoCapture1.Output_Format = tsOutput
                Case 6
                    Dim movOutput = New VFMOVOutput()
                    SetMOVOutput(movOutput)
                    VideoCapture1.Output_Format = movOutput
            End Select
        End If

        VideoCapture1.Video_Effects_Enabled = True
        VideoCapture1.Video_Effects_Clear()
        lbLogos.Items.Clear()
        ConfigureVideoEffects()

        Await VideoCapture1.StartAsync()

        tcMain.SelectedIndex = 3
        tmRecording.Start()
    End Sub
    Private Async Sub btStop_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btStop.Click

        tmRecording.Stop()
        Await VideoCapture1.StopAsync()

    End Sub

    Private Sub llVideoTutorials_LinkClicked(ByVal sender As System.Object, ByVal e As LinkLabelLinkClickedEventArgs) Handles llVideoTutorials.LinkClicked

        Dim startInfo = New ProcessStartInfo("explorer.exe", HelpLinks.VideoTutorials)
        Process.Start(startInfo)

    End Sub

    Private Sub Log(msg As String)
        If (IsHandleCreated) Then
            Invoke(Sub()
                       mmLog.Text = mmLog.Text + msg + Environment.NewLine
                   End Sub)
        End If
    End Sub

    Private Sub VideoCapture1_OnError(ByVal sender As System.Object, ByVal e As ErrorsEventArgs) Handles VideoCapture1.OnError
        Log(e.Message)
    End Sub

    Private Sub VideoCapture1_OnLicenseRequired(sender As Object, e As LicenseEventArgs) Handles VideoCapture1.OnLicenseRequired
        Log("(NOT ERROR) " + e.Message)
    End Sub

    Private Sub linkLabel7_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linkLabel7.LinkClicked

        Dim startInfo = New ProcessStartInfo("explorer.exe", HelpLinks.RedistVLCx86UI)
        Process.Start(startInfo)

    End Sub

    Private Sub btShowIPCamDatabase_Click(sender As Object, e As EventArgs) Handles btShowIPCamDatabase.Click
        IPCameraDB.ShowWindow()
    End Sub

    Private Sub btONVIFConnect_Click(sender As Object, e As EventArgs) Handles btONVIFConnect.Click

        If (btONVIFConnect.Text = "Connect") Then
            btONVIFConnect.Text = "Disconnect"

            If (onvifControl IsNot Nothing) Then
                onvifControl.Disconnect()
                onvifControl.Dispose()
                onvifControl = Nothing
            End If

            If (String.IsNullOrEmpty(edONVIFLogin.Text) Or String.IsNullOrEmpty(edONVIFPassword.Text)) Then
                MessageBox.Show("Please specify IP camera user name and password.")
                Exit Sub
            End If

            onvifControl = New ONVIFControl()
            Dim result = onvifControl.Connect(edONVIFURL.Text, edONVIFLogin.Text, edONVIFPassword.Text)

            If (Not result) Then
                onvifControl = Nothing
                MessageBox.Show("Unable to connect to ONVIF camera.")
                Exit Sub
            End If

            Dim deviceInfo = onvifControl.GetDeviceInformation()
            lbONVIFCameraInfo.Text = $"Model {deviceInfo.Model}, Firmware {deviceInfo.Firmware}"

            cbONVIFProfile.Items.Clear()

            Dim profiles As VisioForge.MediaFramework.ONVIF.Profile() = onvifControl.GetProfiles()
            For Each profile As VisioForge.MediaFramework.ONVIF.Profile In profiles
                cbONVIFProfile.Items.Add($"{profile.Name}")
            Next

            If (cbONVIFProfile.Items.Count > 0) Then
                cbONVIFProfile.SelectedIndex = 0
            End If

            edONVIFLiveVideoURL.Text = onvifControl.GetVideoURL()
            cbIPURL.Text = edONVIFLiveVideoURL.Text

            edIPLogin.Text = edONVIFLogin.Text
            edIPPassword.Text = edONVIFPassword.Text

            onvifPtzRanges = onvifControl.PTZ_GetRanges()
            onvifControl.PTZ_SetAbsolute(0, 0, 0)

            onvifPtzX = 0
            onvifPtzY = 0
            onvifPtzZoom = 0
        Else
            btONVIFConnect.Text = "Connect"

            If (onvifControl IsNot Nothing) Then
                onvifControl.Disconnect()
                onvifControl.Dispose()
                onvifControl = Nothing
            End If
        End If

    End Sub

    Private Sub btONVIFRight_Click(sender As Object, e As EventArgs) Handles btONVIFRight.Click

        If (onvifControl Is Nothing Or onvifPtzRanges Is Nothing) Then
            Exit Sub
        End If

        Dim step_ As Double = (onvifPtzRanges.MaxX - onvifPtzRanges.MinX) / 30
        onvifPtzX = onvifPtzX - step_

        If (onvifPtzX < onvifPtzRanges.MinX) Then
            onvifPtzX = onvifPtzRanges.MinX
        End If

        onvifControl?.PTZ_SetAbsolute(onvifPtzX, onvifPtzY, onvifPtzZoom)

    End Sub

    Private Sub btONVIFPTZSetDefault_Click(sender As Object, e As EventArgs) Handles btONVIFPTZSetDefault.Click

        onvifControl?.PTZ_SetAbsolute(0, 0, 0)

    End Sub

    Private Sub btONVIFLeft_Click(sender As Object, e As EventArgs) Handles btONVIFLeft.Click

        If (onvifControl Is Nothing Or onvifPtzRanges Is Nothing) Then
            Exit Sub
        End If

        Dim step_ As Double = (onvifPtzRanges.MaxX - onvifPtzRanges.MinX) / 30
        onvifPtzX = onvifPtzX + step_

        If (onvifPtzX > onvifPtzRanges.MaxX) Then
            onvifPtzX = onvifPtzRanges.MaxX
        End If

        onvifControl?.PTZ_SetAbsolute(onvifPtzX, onvifPtzY, onvifPtzZoom)

    End Sub

    Private Sub btONVIFUp_Click(sender As Object, e As EventArgs) Handles btONVIFUp.Click

        If (onvifControl Is Nothing Or onvifPtzRanges Is Nothing) Then
            Exit Sub
        End If

        Dim step_ As Double = (onvifPtzRanges.MaxY - onvifPtzRanges.MinY) / 30
        onvifPtzY = onvifPtzY - step_

        If (onvifPtzY < onvifPtzRanges.MinY) Then
            onvifPtzY = onvifPtzRanges.MinY
        End If

        onvifControl?.PTZ_SetAbsolute(onvifPtzX, onvifPtzY, onvifPtzZoom)

    End Sub

    Private Sub btONVIFDown_Click(sender As Object, e As EventArgs) Handles btONVIFDown.Click

        If (onvifControl Is Nothing Or onvifPtzRanges Is Nothing) Then
            Exit Sub
        End If

        Dim step_ As Double = (onvifPtzRanges.MaxY - onvifPtzRanges.MinY) / 30
        onvifPtzY = onvifPtzY + step_

        If (onvifPtzY > onvifPtzRanges.MaxY) Then
            onvifPtzY = onvifPtzRanges.MaxY
        End If

        onvifControl?.PTZ_SetAbsolute(onvifPtzX, onvifPtzY, onvifPtzZoom)

    End Sub

    Private Sub btONVIFZoomIn_Click(sender As Object, e As EventArgs) Handles btONVIFZoomIn.Click

        If (onvifControl Is Nothing Or onvifPtzRanges Is Nothing) Then
            Exit Sub
        End If

        Dim step_ As Double = (onvifPtzRanges.MaxZoom - onvifPtzRanges.MinZoom) / 100
        onvifPtzZoom = onvifPtzZoom + step_

        If (onvifPtzZoom > onvifPtzRanges.MaxZoom) Then
            onvifPtzZoom = onvifPtzRanges.MaxZoom
        End If

        onvifControl?.PTZ_SetAbsolute(onvifPtzX, onvifPtzY, onvifPtzZoom)

    End Sub

    Private Sub btONVIFZoomOut_Click(sender As Object, e As EventArgs) Handles btONVIFZoomOut.Click

        If (onvifControl Is Nothing Or onvifPtzRanges Is Nothing) Then
            Exit Sub
        End If

        Dim step_ As Double = (onvifPtzRanges.MaxZoom - onvifPtzRanges.MinZoom) / 100
        onvifPtzZoom = onvifPtzZoom - step_

        If (onvifPtzZoom < onvifPtzRanges.MinZoom) Then
            onvifPtzZoom = onvifPtzRanges.MinZoom
        End If

        onvifControl?.PTZ_SetAbsolute(onvifPtzX, onvifPtzY, onvifPtzZoom)

    End Sub

    Private Sub cbOutputFormat_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbOutputFormat.SelectedIndexChanged
        Select Case (cbOutputFormat.SelectedIndex)
            Case 0
                edOutput.Text = FilenameHelper.ChangeFileExt(edOutput.Text, ".avi")
            Case 1
                edOutput.Text = FilenameHelper.ChangeFileExt(edOutput.Text, ".wmv")
            Case 2
                edOutput.Text = FilenameHelper.ChangeFileExt(edOutput.Text, ".mp4")
            Case 3
                edOutput.Text = FilenameHelper.ChangeFileExt(edOutput.Text, ".mp4")
            Case 4
                edOutput.Text = FilenameHelper.ChangeFileExt(edOutput.Text, ".gif")
            Case 5
                edOutput.Text = FilenameHelper.ChangeFileExt(edOutput.Text, ".ts")
            Case 6
                edOutput.Text = FilenameHelper.ChangeFileExt(edOutput.Text, ".mov")
        End Select
    End Sub

    Private Async Sub btResume_Click(sender As Object, e As EventArgs) Handles btResume.Click
        Await VideoCapture1.ResumeAsync()
    End Sub

    Private Async Sub btPause_Click(sender As Object, e As EventArgs) Handles btPause.Click
        Await VideoCapture1.PauseAsync()
    End Sub

    Private Sub UpdateRecordingTime()
        If Me.IsHandleCreated Then
            Dim ts = VideoCapture1.Duration_Time()

            If (Math.Abs(ts.TotalMilliseconds) < 0.01) Then
                Return
            End If

            BeginInvoke(Sub()
                            lbTimestamp.Text = $"Recording time: " + String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds)
                        End Sub)
        End If
    End Sub

    Private Sub btOutputConfigure_Click(sender As Object, e As EventArgs) Handles btOutputConfigure.Click
        Select Case (cbOutputFormat.SelectedIndex)
            Case 0
                If (aviSettingsDialog Is Nothing) Then
                    aviSettingsDialog = New AVISettingsDialog(VideoCapture1.Video_Codecs.ToArray(), VideoCapture1.Audio_Codecs.ToArray())
                End If

                aviSettingsDialog.ShowDialog(Me)
            Case 1
                If (wmvSettingsDialog Is Nothing) Then
                    wmvSettingsDialog = New WMVSettingsDialog(VideoCapture1)
                End If

                wmvSettingsDialog.WMA = False
                wmvSettingsDialog.ShowDialog(Me)
            Case 2
                If (mp4V10SettingsDialog Is Nothing) Then
                    mp4V10SettingsDialog = New MP4v10SettingsDialog()
                End If

                mp4V10SettingsDialog.ShowDialog(Me)
            Case 3
                If (mp4v11SettingsDialog Is Nothing) Then
                    mp4v11SettingsDialog = New MFSettingsDialog(MFSettingsDialogMode.MP4v11)
                End If

                mp4v11SettingsDialog.ShowDialog(Me)
            Case 4
                If (gifSettingsDialog Is Nothing) Then
                    gifSettingsDialog = New GIFSettingsDialog()
                End If

                gifSettingsDialog.ShowDialog(Me)
            Case 5
                If (mpegTSSettingsDialog Is Nothing) Then
                    mpegTSSettingsDialog = New MFSettingsDialog(MFSettingsDialogMode.MPEGTS)
                End If

                mpegTSSettingsDialog.ShowDialog(Me)
            Case 6
                If (movSettingsDialog Is Nothing) Then
                    movSettingsDialog = New MFSettingsDialog(MFSettingsDialogMode.MOV)
                End If

                movSettingsDialog.ShowDialog(Me)
        End Select
    End Sub

    Private Async Sub btSaveScreenshot_Click(sender As Object, e As EventArgs) Handles btSaveScreenshot.Click
        If (screenshotSaveDialog.ShowDialog(Me) = DialogResult.OK) Then
            Dim filename = screenshotSaveDialog.FileName
            Dim ext = Path.GetExtension(filename)?.ToLowerInvariant()
            Select Case (ext)
                Case ".bmp"
                    Await VideoCapture1.Frame_SaveAsync(filename, VFImageFormat.BMP, 0)
                Case ".jpg"
                    Await VideoCapture1.Frame_SaveAsync(filename, VFImageFormat.JPEG, 85)
                Case ".gif"
                    Await VideoCapture1.Frame_SaveAsync(filename, VFImageFormat.GIF, 0)
                Case ".png"
                    Await VideoCapture1.Frame_SaveAsync(filename, VFImageFormat.PNG, 0)
                Case ".tiff"
                    Await VideoCapture1.Frame_SaveAsync(filename, VFImageFormat.TIFF, 0)
            End Select
        End If
    End Sub

    Private Sub ConfigureVideoEffects()

        'Other effects
        If tbLightness.Value > 0 Then
            tbLightness_Scroll(Nothing, Nothing)
        End If

        If tbSaturation.Value < 255 Then
            tbSaturation_Scroll(Nothing, Nothing)
        End If

        If tbContrast.Value > 0 Then
            tbContrast_Scroll(Nothing, Nothing)
        End If

        If tbDarkness.Value > 0 Then
            tbDarkness_Scroll(Nothing, Nothing)
        End If

        If cbGreyscale.Checked Then
            cbGreyscale_CheckedChanged(Nothing, Nothing)
        End If

        If cbInvert.Checked Then
            cbInvert_CheckedChanged(Nothing, Nothing)
        End If

        If cbFlipX.Checked Then
            cbFlipX_CheckedChanged(Nothing, Nothing)
        End If

        If cbFlipY.Checked Then
            cbFlipY_CheckedChanged(Nothing, Nothing)
        End If
    End Sub

    Private Sub tbLightness_Scroll(ByVal sender As System.Object, ByVal e As EventArgs) Handles tbLightness.Scroll

        Dim intf As IVFVideoEffectLightness
        Dim effect = VideoCapture1.Video_Effects_Get("Lightness")
        If (IsNothing(effect)) Then
            intf = New VFVideoEffectLightness(True, tbLightness.Value)
            VideoCapture1.Video_Effects_Add(intf)
        Else
            intf = effect
            If (Not IsNothing(intf)) Then
                intf.Value = tbLightness.Value
            End If
        End If

    End Sub

    Private Sub tbSaturation_Scroll(ByVal sender As System.Object, ByVal e As EventArgs) Handles tbSaturation.Scroll

        Dim intf As IVFVideoEffectSaturation
        Dim effect = VideoCapture1.Video_Effects_Get("Saturation")
        If (IsNothing(effect)) Then
            intf = New VFVideoEffectSaturation(tbSaturation.Value)
            VideoCapture1.Video_Effects_Add(intf)
        Else

            intf = effect
            If (Not IsNothing(intf)) Then
                intf.Value = tbSaturation.Value
            End If
        End If

    End Sub

    Private Sub tbContrast_Scroll(ByVal sender As System.Object, ByVal e As EventArgs) Handles tbContrast.Scroll

        Dim intf As IVFVideoEffectContrast
        Dim effect = VideoCapture1.Video_Effects_Get("Contrast")
        If (IsNothing(effect)) Then
            intf = New VFVideoEffectContrast(True, tbContrast.Value)
            VideoCapture1.Video_Effects_Add(intf)
        Else
            intf = effect
            If (Not IsNothing(intf)) Then
                intf.Value = tbContrast.Value
            End If
        End If

    End Sub

    Private Sub tbDarkness_Scroll(ByVal sender As System.Object, ByVal e As EventArgs) Handles tbDarkness.Scroll

        Dim intf As IVFVideoEffectDarkness
        Dim effect = VideoCapture1.Video_Effects_Get("Darkness")
        If (IsNothing(effect)) Then
            intf = New VFVideoEffectDarkness(True, tbDarkness.Value)
            VideoCapture1.Video_Effects_Add(intf)
        Else
            intf = effect
            If (Not IsNothing(intf)) Then
                intf.Value = tbDarkness.Value
            End If
        End If

    End Sub

    Private Sub cbGreyscale_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles cbGreyscale.CheckedChanged

        Dim intf As IVFVideoEffectGrayscale
        Dim effect = VideoCapture1.Video_Effects_Get("Grayscale")
        If (IsNothing(effect)) Then
            intf = New VFVideoEffectGrayscale(cbGreyscale.Checked)
            VideoCapture1.Video_Effects_Add(intf)
        Else
            intf = effect
            If (Not IsNothing(intf)) Then
                intf.Enabled = cbGreyscale.Checked
            End If
        End If

    End Sub

    Private Sub cbInvert_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles cbInvert.CheckedChanged

        Dim intf As IVFVideoEffectInvert
        Dim effect = VideoCapture1.Video_Effects_Get("Invert")
        If (IsNothing(effect)) Then
            intf = New VFVideoEffectInvert(cbInvert.Checked)
            VideoCapture1.Video_Effects_Add(intf)
        Else
            intf = effect
            If (Not IsNothing(intf)) Then
                intf.Enabled = cbInvert.Checked
            End If
        End If

    End Sub

    Private Sub cbFlipX_CheckedChanged(sender As Object, e As EventArgs) Handles cbFlipX.CheckedChanged
        Dim flip As IVFVideoEffectFlipDown
        Dim effect = VideoCapture1.Video_Effects_Get("FlipDown")
        If (effect Is Nothing) Then
            flip = New VFVideoEffectFlipHorizontal(cbFlipX.Checked)
            VideoCapture1.Video_Effects_Add(flip)
        Else
            flip = effect
            If (flip IsNot Nothing) Then
                flip.Enabled = cbFlipX.Checked
            End If
        End If
    End Sub

    Private Sub cbFlipY_CheckedChanged(sender As Object, e As EventArgs) Handles cbFlipY.CheckedChanged
        Dim flip As IVFVideoEffectFlipRight
        Dim effect = VideoCapture1.Video_Effects_Get("FlipRight")
        If (effect Is Nothing) Then
            flip = New VFVideoEffectFlipVertical(cbFlipY.Checked)
            VideoCapture1.Video_Effects_Add(flip)
        Else
            flip = effect
            If (flip IsNot Nothing) Then
                flip.Enabled = cbFlipY.Checked
            End If
        End If
    End Sub

    Private Sub btImageLogoAdd_Click(sender As Object, e As EventArgs) Handles btImageLogoAdd.Click
        Dim dlg = New ImageLogoSettingsDialog()

        Dim effectName = dlg.GenerateNewEffectName(VideoCapture1)
        Dim effect = New VFVideoEffectImageLogo(True, effectName)

        VideoCapture1.Video_Effects_Add(effect)
        lbLogos.Items.Add(effect.Name)

        dlg.Fill(effect)
        dlg.ShowDialog(Me)
        dlg.Dispose()
    End Sub

    Private Sub btTextLogoAdd_Click(sender As Object, e As EventArgs) Handles btTextLogoAdd.Click
        Dim dlg = New TextLogoSettingsDialog()

        Dim effectName = dlg.GenerateNewEffectName(VideoCapture1)
        Dim effect = New VFVideoEffectTextLogo(True, effectName)

        VideoCapture1.Video_Effects_Add(effect)
        lbLogos.Items.Add(effect.Name)
        dlg.Fill(effect)

        dlg.ShowDialog(Me)
        dlg.Dispose()
    End Sub

    Private Sub btLogoEdit_Click(sender As Object, e As EventArgs) Handles btLogoEdit.Click
        If (lbLogos.SelectedItem IsNot Nothing) Then
            Dim effect = VideoCapture1.Video_Effects_Get(lbLogos.SelectedItem)
            If (effect.GetEffectType() = VFVideoEffectType.TextLogo) Then
                Dim dlg = New TextLogoSettingsDialog()

                dlg.Attach(effect)

                dlg.ShowDialog(Me)
                dlg.Dispose()
            ElseIf (effect.GetEffectType() = VFVideoEffectType.ImageLogo) Then
                Dim dlg = New ImageLogoSettingsDialog()

                dlg.Attach(effect)

                dlg.ShowDialog(Me)
                dlg.Dispose()
            End If
        End If
    End Sub

    Private Sub btLogoRemove_Click(sender As Object, e As EventArgs) Handles btLogoRemove.Click
        If (lbLogos.SelectedItem IsNot Nothing) Then
            VideoCapture1.Video_Effects_Remove(lbLogos.SelectedItem)
            lbLogos.Items.Remove(lbLogos.SelectedItem)
        End If
    End Sub

    Private Sub linkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linkLabel1.LinkClicked
        Dim startInfo = New ProcessStartInfo("explorer.exe", HelpLinks.RedistVLCx64UI)
        Process.Start(startInfo)
    End Sub

    Private Sub lbNDI_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles lbNDI.LinkClicked
        Dim startInfo As ProcessStartInfo = New ProcessStartInfo("explorer.exe", HelpLinks.NDIVendor)
        Process.Start(startInfo)
    End Sub

    Private Sub btListNDISources_Click(sender As Object, e As EventArgs) Handles btListNDISources.Click
        cbIPURL.Items.Clear()

        Dim lst As Uri() = VideoCapture1.IP_Camera_NDI_ListSources()
        For Each uri As Uri In lst
            cbIPURL.Items.Add(uri)
        Next

        If (cbIPURL.Items.Count > 0) Then
            cbIPURL.SelectedIndex = 0
        End If
    End Sub
End Class

' ReSharper restore InconsistentNaming