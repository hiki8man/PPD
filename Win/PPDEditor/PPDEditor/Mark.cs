using PPDEditor.Models;
using PPDFramework;
using PPDFramework.PPDStructure.EVDData;
using PPDFramework.PPDStructure.PPDData;
using System.Numerics;
using PPDMath;
using Matrix = System.Numerics.Matrix4x4;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PPDEditor
{
    public class Mark : GameComponent
    {
        public static float defaultbpm => PPDStaticSetting.ScriptMode == EditorScriptMode.DivaScript ? 120f : 130f;
        protected const float autowidth = 1 / 120f;
        protected float x;
        protected float y;
        protected float angle;
        protected float time;
        protected NoteType noteType;
        protected int sameTimingMarks;
        protected int sameTimingMarksCount;
        protected Matrix m;
        protected ButtonType type;
        protected PictureObject mark;
        protected PictureObject markc;
        protected PictureObject jiku;
        protected PPDEditorSkin skin;
        protected PPDFramework.Resource.ResourceManager resourceManager;
        protected Dictionary<string, string> parameters;
        public Mark(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, PPDEditorSkin skin, ButtonType type, float x, float y, float time, float angle, uint id) : base(device)
        {
            this.x = x;
            this.y = y;
            this.angle = angle;
            this.time = time;
            this.skin = skin;
            this.resourceManager = resourceManager;
            parameters = [];
            mark = new PictureObject(device, resourceManager, skin.GetMarkImagePath(type), true)
            {
                Position = new Vector2(x, y)
            };
            markc = new PictureObject(device, resourceManager, skin.GetMarkColorImagePath(type))
            {
                Position = new Vector2(x, y)
            };
            jiku = new PictureObject(device, resourceManager, skin.GetClockAxisImagePath(), true)
            {
                Position = new Vector2(x, y)
            };
            this.m = Matrix.Transformation2D(Vector2.Zero, 0, Vector2.One, new Vector2(this.x, this.y), -angle, Vector2.Zero);
            Hidden = true;
            mark.Scale = Vector2.One;
            this.type = type;
            this.ID = id;
        }

        protected void UpdateSameTimingMarks(int sameTimingMarks)
        {
            sameTimingMarksCount = 0;
            for (int i = 0; i < 10; i++)
            {
                if (((sameTimingMarks >> i) & 1) == 1)
                {
                    sameTimingMarksCount++;
                }
            }
            SameTimingMarks = sameTimingMarks;
        }

        public int Update(float ctime, float bp, DisplayState dstate, NoteType noteType, int sameTimingMarks)
        {
            NoteType = noteType;
            UpdateSameTimingMarks(sameTimingMarks);
            int ret = 0;
            if (!Hidden && ctime - this.time >= -autowidth)
            {
                ret = 1;
            }
            if (ctime - this.time >= -autowidth)
            {
                Hidden = true;
                return ret;
            }
            float scale = (float)bp / defaultbpm;
            if (this.time - ctime <= 2 / scale)
            {
                if (dstate == DisplayState.Normal || dstate == DisplayState.Hidden)
                {
                    Hidden = false;
                }
            }
            else
            {
                Hidden = true;
            }
            if (this.time - ctime <= 0.5f / scale)
            {
                Hidden &= dstate != DisplayState.Sudden;
            }
            if (this.time - ctime <= 0.25f / scale)
            {
                if (dstate == DisplayState.Hidden)
                {
                    Hidden = true;
                }
            }
            float positionScale = PPDStaticSetting.ScriptMode == EditorScriptMode.Vet08H
                ? (bp - PPDStaticSetting.SubtractBPM) / defaultbpm
                : scale;
            markc.Position = CalcurateMarkCPosition(Position, markc, this.time, ctime, positionScale, m);
            jiku.Rotation = -1 * (float)Math.PI * (this.time - ctime) * scale;
            if (jiku.Rotation >= 0)
            {
                jiku.Rotation = 0;
            }
            return ret;
        }

        protected Vector2 CalcurateMarkCPosition(Vector2 position, PictureObject colorMark, float markTime, float currentTime, float scale)
        {
            return CalcurateMarkCPosition(position, colorMark, markTime, currentTime, scale,
                 Matrix.Transformation2D(Vector2.Zero, 0, Vector2.One, position, -angle, Vector2.Zero));
        }

        protected Vector2 CalcurateMarkCPosition(Vector2 position, PictureObject colorMark, float markTime, float currentTime, float scale, Matrix m)
        {
            Vector2 vec = position + (sameTimingMarksCount > 1 ?
                ChangeColorPositionStraight(scale, markTime - currentTime) :
                ChangeColorPosition(scale, markTime - currentTime));
            vec = Vector2.TransformCoordinate(vec, m);
            vec = new Vector2(vec.X - colorMark.Width / 2, vec.Y - colorMark.Height / 2);
            return vec;
        }

        public virtual void GraphicUpdate()
        {
            mark.Update();
            jiku.Update();
            markc.Update();
        }

        protected Vector2 ChangeColorPositionStraight(float scale, float timediff)
        {
            return PPDStaticSetting.ScriptMode switch
            {
                EditorScriptMode.DivaScript => ChangeColorPositionStraightDivaScript(scale, timediff),
                EditorScriptMode.Vet08 => ChangeColorPositionStraightVet08(scale, timediff),
                _ => ChangeColorPositionStraightVet08H(scale, timediff)
            };
        }

        protected Vector2 ChangeColorPosition(float scale, float timediff)
        {
            return PPDStaticSetting.ScriptMode switch
            {
                EditorScriptMode.DivaScript => ChangeColorPositionDivaScript(scale, timediff),
                EditorScriptMode.Vet08 => ChangeColorPositionVet08(scale, timediff),
                _ => ChangeColorPositionVet08H(scale, timediff)
            };
        }

        private Vector2 ChangeColorPositionStraightDivaScript(float scale, float timediff)
        {
            double frequency;
            if (parameters.TryGetValue("Frequency", out string frequencyParm))
            {
                _ = double.TryParse(frequencyParm, out frequency);
                frequency = parameters.ContainsKey("#RightRotation") ? frequency : -frequency;
            }
            else
            {
                frequency = 0;
            }
            float amplitude;
            if (parameters.TryGetValue("Amplitude", out string amplitudeParm))
            {
                _ = float.TryParse(amplitudeParm, out amplitude);
            }
            else
            {
                amplitude = 500f;
            }
            float distance;
            if (parameters.TryGetValue("Distance", out string distanceParm))
            {
                _ = float.TryParse(distanceParm, out distance);
            }
            else
            {
                distance = 220000f;
            }
            return new Vector2(distance * 250f / 300000f * timediff * scale, (float)Math.Sin((double)(timediff * scale) * Math.PI * frequency * 0.5) * amplitude / 28.8f);
        }

        private Vector2 ChangeColorPositionDivaScript(float scale, float timediff)
        {
            double frequency;
            if (parameters.TryGetValue("Frequency", out string frequencyParm))
            {
                if (double.TryParse(frequencyParm, out frequency))
                {
                    frequency = parameters.ContainsKey("#RightRotation") ? frequency : -frequency;
                }
                else
                {
                    frequency = 0;
                }
            }
            else
            {
                frequency = parameters.ContainsKey("#RightRotation") ? 2 : -2;
            }
            float amplitude;
            if (parameters.TryGetValue("Amplitude", out string amplitudeParm))
            {
                _ = float.TryParse(amplitudeParm, out amplitude);
            }
            else
            {
                amplitude = 500f;
            }
            float distance;
            if (parameters.TryGetValue("Distance", out string distanceParm))
            {
                _ = float.TryParse(distanceParm, out distance);
            }
            else
            {
                distance = 300000f;
            }
            return new Vector2(distance * 250f / 300000f * timediff * scale, (float)Math.Sin((double)(timediff * scale) * Math.PI * frequency * 0.5) * amplitude / 28.8f);
        }

        private Vector2 ChangeColorPositionStraightVet08(float scale, float timediff)
        {
            float decelerationRate;
            if (parameters.TryGetValue("DecelerationRate", out string decelerationRateParm))
            {
                _ = float.TryParse(decelerationRateParm, out decelerationRate);
            }
            else
            {
                decelerationRate = PPDStaticSetting.Vet08DecelerationRate;
            }
            float markDistance;
            if (parameters.TryGetValue("MarkDistance", out string markDistanceParm))
            {
                _ = float.TryParse(markDistanceParm, out markDistance);
                markDistance *= decelerationRate;
            }
            else
            {
                markDistance = PPDStaticSetting.Vet08MarkDistance * decelerationRate;
            }
            float curvature;
            if (parameters.TryGetValue("Curvature", out string curvatureParm))
            {
                _ = float.TryParse(curvatureParm, out curvature);
                curvature = parameters.ContainsKey("#RightRotation") ? curvature : -curvature;
            }
            else
            {
                curvature = parameters.ContainsKey("#RightRotation") ? PPDStaticSetting.Vet08CurvatureMulti : -PPDStaticSetting.Vet08CurvatureMulti;
            }
            float markSpeed;
            if (parameters.TryGetValue("MarkSpeed", out string markSpeedParm))
            {
                _ = float.TryParse(markSpeedParm, out markSpeed);
            }
            else
            {
                markSpeed = PPDStaticSetting.Vet08MarkSpeed;
            }
            return new Vector2(markDistance * timediff * scale * markSpeed, curvature * (float)Math.Sin(timediff * scale * Math.PI * markSpeed));
        }

        private Vector2 ChangeColorPositionVet08(float scale, float timediff)
        {
            float markDistance;
            if (parameters.TryGetValue("MarkDistance", out string markDistanceParm))
            {
                _ = float.TryParse(markDistanceParm, out markDistance);
            }
            else
            {
                markDistance = PPDStaticSetting.Vet08MarkDistance;
            }
            float curvature;
            if (parameters.TryGetValue("Curvature", out string curvatureParm))
            {
                _ = float.TryParse(curvatureParm, out curvature);
                curvature = parameters.ContainsKey("#RightRotation") ? curvature : -curvature;
            }
            else
            {
                curvature = parameters.ContainsKey("#RightRotation") ? PPDStaticSetting.Vet08Curvature : -PPDStaticSetting.Vet08Curvature;
            }
            float markSpeed;
            if (parameters.TryGetValue("MarkSpeed", out string markSpeedParm))
            {
                _ = float.TryParse(markSpeedParm, out markSpeed);
            }
            else
            {
                markSpeed = PPDStaticSetting.Vet08MarkSpeed;
            }
            return new Vector2(markDistance * timediff * scale * markSpeed, curvature * (float)Math.Sin(timediff * scale * Math.PI * markSpeed));
        }

        private Vector2 ChangeColorPositionStraightVet08H(float scale, float timediff)
        {
            float markDistanceRatio;
            if (parameters.TryGetValue("MarkDistanceRatio", out string markDistanceRatioParm))
            {
                _ = float.TryParse(markDistanceRatioParm, out markDistanceRatio);
            }
            else
            {
                markDistanceRatio = PPDStaticSetting.MarkDistanceRatio;
            }
            float markDistance = PPDStaticSetting.Vet08HMarkDistance * PPDStaticSetting.Vet08HDecelerationRate * markDistanceRatio;

            float curvature;
            if (parameters.TryGetValue("Curvature", out string curvatureParm))
            {
                _ = float.TryParse(curvatureParm, out curvature);
                curvature = parameters.ContainsKey("#RightRotation") ? curvature : -curvature;
            }
            else
            {
                curvature = parameters.ContainsKey("#RightRotation") ? PPDStaticSetting.Vet08HCurvatureMulti : -PPDStaticSetting.Vet08HCurvatureMulti;
            }
            float markSpeed;
            if (parameters.TryGetValue("MarkSpeed", out string markSpeedParm))
            {
                _ = float.TryParse(markSpeedParm, out markSpeed);
            }
            else
            {
                markSpeed = PPDStaticSetting.Vet08HMarkSpeed;
            }
            float frequency;
            if (parameters.TryGetValue("Frequency", out string frequencyParm))
            {
                _ = float.TryParse(frequencyParm, out frequency);
            }
            else
            {
                frequency = PPDStaticSetting.Frequency;
            }
            return new Vector2(markDistance * timediff * scale * markSpeed, curvature * (float)Math.Sin(timediff * scale * markSpeed * Math.PI * frequency * 0.5f));
        }

        private Vector2 ChangeColorPositionVet08H(float scale, float timediff)
        {
            float markDistanceRatio;
            if (parameters.TryGetValue("MarkDistanceRatio", out string markDistanceRatioParm))
            {
                _ = float.TryParse(markDistanceRatioParm, out markDistanceRatio);
            }
            else
            {
                markDistanceRatio = PPDStaticSetting.MarkDistanceRatio;
            }
            float markDistance = PPDStaticSetting.Vet08HMarkDistance * markDistanceRatio;

            float curvature;
            if (parameters.TryGetValue("Curvature", out string curvatureParm))
            {
                _ = float.TryParse(curvatureParm, out curvature);
                curvature = parameters.ContainsKey("#RightRotation") ? curvature : -curvature;
            }
            else
            {
                curvature = parameters.ContainsKey("#RightRotation") ? PPDStaticSetting.Vet08HCurvature : -PPDStaticSetting.Vet08HCurvature;
            }
            float markSpeed;
            if (parameters.TryGetValue("MarkSpeed", out string markSpeedParm))
            {
                _ = float.TryParse(markSpeedParm, out markSpeed);
            }
            else
            {
                markSpeed = PPDStaticSetting.Vet08HMarkSpeed;
            }
            float frequency;
            if (parameters.TryGetValue("Frequency", out string frequencyParm))
            {
                _ = float.TryParse(frequencyParm, out frequency);
            }
            else
            {
                frequency = PPDStaticSetting.Frequency;
            }
            return new Vector2(markDistance * timediff * scale * markSpeed, curvature * (float)Math.Sin(timediff * scale * markSpeed * Math.PI * frequency * 0.5f));
        }

        public override void Draw()
        {
            if (Hidden) return;
            mark.Draw();
            jiku.Draw();
        }

        public virtual void ColorDraw()
        {
            markc.Draw();
        }

        public virtual void DrawOnlyMark()
        {
            mark.Draw();
        }
        protected ButtonType GetButtonType()
        {
            if (noteType == PPDFramework.NoteType.ACFT)
            {
                switch (type)
                {
                    case ButtonType.Triangle:
                        if (HasRLSameTiming)
                        {
                            goto case ButtonType.L;
                        }
                        break;
                    case ButtonType.Circle:
                        if (HasRLSameTiming)
                        {
                            goto case ButtonType.R;
                        }
                        break;
                    case ButtonType.R:
                        return ButtonType.Start;
                    case ButtonType.L:
                        return ButtonType.Start + 1;
                }
            }
            return type;
        }

        protected virtual void UpdateMarkTime()
        {

        }

        public void RefreshSkinImages()
        {
            jiku = new PictureObject(device, resourceManager, skin.GetClockAxisImagePath(), true)
            {
                Position = new Vector2(x, y)
            };
            UpdateMarkImage();
        }

        protected virtual void UpdateMarkImage()
        {
            int noteType = (int)GetButtonType();
            if (sameTimingMarksCount > 1)
            {
                noteType += 14;
            }
            mark = new PictureObject(device, resourceManager, skin.GetMarkImagePath(noteType), true)
            {
                Position = new Vector2(x, y)
            };
            markc = new PictureObject(device, resourceManager, skin.GetMarkColorImagePath(noteType))
            {
                Position = new Vector2(x, y)
            };
        }
        protected virtual void UpdateMarkPosition()
        {
            mark.Position = new Vector2(this.x, this.y);
            jiku.Position = new Vector2(this.x, this.y);
            UpdateTransform();
        }
        protected virtual void UpdateTransform()
        {
            this.m = Matrix.Transformation2D(Vector2.Zero, 0, Vector2.One, new Vector2(this.x, this.y), -angle, Vector2.Zero);
        }

        public virtual MarkData Convert()
        {
            var ret = new MarkData(x, y, angle, time, type, ID);
            foreach (var parameter in parameters)
            {
                ret.AddParameter(parameter.Key, parameter.Value);
            }
            return ret;
        }

        public override Vector2 Position
        {
            get
            {
                return new Vector2(this.x, this.y);
            }
            set
            {
                this.x = value.X;
                this.y = value.Y;
                UpdateMarkPosition();
            }
        }

        public override float Rotation
        {
            get
            {
                return angle;
            }
            set
            {
                if (angle != value)
                {
                    angle = value;
                    UpdateTransform();
                }
            }
        }

        public virtual ButtonType Type
        {
            get
            {
                return type;
            }
            set
            {
                if (type != value)
                {
                    type = value;
                    UpdateMarkImage();
                }
            }
        }
        public virtual NoteType NoteType
        {
            get
            {
                return noteType;
            }
            set
            {
                if (noteType != value)
                {
                    noteType = value;
                    UpdateMarkImage();
                }
            }
        }
        public virtual int SameTimingMarks
        {
            get
            {
                return sameTimingMarks;
            }
            set
            {
                if (sameTimingMarks != value)
                {
                    sameTimingMarks = value;
                    UpdateMarkImage();
                }
            }
        }
        public bool HasRLSameTiming
        {
            get
            {
                return ((sameTimingMarks >> (int)ButtonType.R) & 1) == 1 ||
                     ((sameTimingMarks >> (int)ButtonType.L) & 1) == 1;
            }
        }
        public bool IsScratch
        {
            get
            {
                if (noteType != PPDFramework.NoteType.ACFT)
                {
                    return false;
                }
                switch (type)
                {
                    case ButtonType.Triangle:
                    case ButtonType.Circle:
                        if (HasRLSameTiming)
                        {
                            return true;
                        }
                        break;
                    case ButtonType.R:
                        return true;
                    case ButtonType.L:
                        return true;
                }
                return false;
            }
        }
        public bool IsRight
        {
            get
            {
                return GetButtonType() == ButtonType.Start;
            }
        }

        public float Time
        {
            get
            {
                return time;
            }
            set
            {
                time = value;
            }
        }

        public override float Alpha
        {
            get;
            set;
        }

        public override bool Hidden
        {
            get;
            set;
        }

        public uint ID
        {
            get;
            set;
        }

        public KeyValuePair<string, string>[] Parameters
        {
            get { return [.. parameters]; }
        }

        public string this[string key]
        {
            get
            {
                if (!parameters.TryGetValue(key, out string val))
                {
                    val = null;
                }
                return val;
            }
            set
            {
                parameters[key] = value;
            }
        }

        public void RemoveParameter(string key)
        {
            parameters.Remove(key);
        }

        public void CopyParameters(Mark mark)
        {
            foreach (var parameter in mark.Parameters)
            {
                parameters[parameter.Key] = parameter.Value;
            }
        }

        public void CopyParameters(MarkDataBase mark)
        {
            foreach (var parameter in mark.Parameters)
            {
                parameters[parameter.Key] = parameter.Value;
            }
        }
    }
}
