using StereoKit;
using System;
using BoxelModeler.App;

namespace BoxelModeler
{
    /*
     * PC �ł̑���
     * �Q�l�Fhttps://stereokit.net/Pages/Guides/Using-The-Simulator.html
     *  - �}�E�X�̈ړ��F��̏㉺���E�ړ�
     *  - �}�E�X�z�C�[���F��̉��s���ړ�
     *  - �}�E�X�̍��N���b�N�F����
     *  - �}�E�X�̉E�N���b�N�F��
     *  - �}�E�X�̍��N���b�N + �E�N���b�N�F������
     *  - Shift(�܂���Caps Lock) + �}�E�X�̉E�N���b�N + �}�E�X�̈ړ��F���̉�]
     *  - Shift(�܂���Caps Lock) + W A S D Q E�L�[�F���̈ړ�
     *  - Alt + �}�E�X����F�A�C�g���b�L���O�̃V�~�����[�g
     */
    class Program
    {
        static void Main(string[] args)
        {
            // StereoKit ������������
            SKSettings settings = new SKSettings
            {
                // �A�v����
                appName = "BoxelModeler",
                // �A�Z�b�g�t�H���_�̑��΃p�X�iStereoKit �͂��̔z���̃A�Z�b�g��T���ɍs���j
                assetsFolder = "Assets",
            };

            // �������Ɏ��s�����ꍇ�̓A�v���I������
            if (!SK.Initialize(settings))
            {
                Environment.Exit(1);
            }

            // �A�v������
            var app = new BoxelModelerApp();
            app.Initialize();
            Action step = app.Update;

            // �A�v���̃��C�����[�v
            // �����̃R�[���o�b�N�����́E�V�X�e���C�x���g�̌�A�`��C�x���g�̑O�ɖ��t���[���Ă΂��
            while (SK.Step(step)) { }

            app.Shutdown();

            // �A�v���I�������iStereoKit �ƑS�Ẵ��\�[�X���N���[���A�b�v����j
            SK.Shutdown();
        }
    }
}
