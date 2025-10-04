using System.Collections.Generic;

namespace NodeEditor.DataRenderer2D
{
    public class UILine : UIDataMesh, ISpline
    {
        public Spline line;

        Spline ISpline.Line => line;

        IEnumerable<IMesh> m_Drawer = null;

        protected override IEnumerable<IMesh> DrawerFactory
            => m_Drawer ??= LineBuilder.CreateNormal(this).Draw();

        protected override void Start()
        {
            base.Start();
            line.owner = this;
            line.EditCallBack += GeometyUpdateFlagUp;
        }
    }
}