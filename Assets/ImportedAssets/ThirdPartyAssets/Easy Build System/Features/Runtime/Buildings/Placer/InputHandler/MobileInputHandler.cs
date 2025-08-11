/// <summary>
/// Project : Easy Build System
/// Class : MobileInputHandler.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

namespace EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler
{
    public class MobileInputHandler : BaseInputHandler
    {
        #region Internal Methods

        public void HandleBuildModes()
        {
            if (BuildingPlacer.Instance.GetBuildMode == BuildingPlacer.BuildMode.PLACE)
            {
                BuildingPlacer.Instance.PlacingBuildingPart();
            }
            else if (BuildingPlacer.Instance.GetBuildMode == BuildingPlacer.BuildMode.DESTROY)
            {
                BuildingPlacer.Instance.DestroyBuildingPart();
            }
            else if (BuildingPlacer.Instance.GetBuildMode == BuildingPlacer.BuildMode.EDIT)
            {
                BuildingPlacer.Instance.EditingBuildingPart();
            }
        }

        public void RotatePreview()
        {
            BuildingPlacer.Instance.RotatePreview();
        }

        public void CancelBuildMode()
        {

            BuildingPlacer.Instance.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
        }

        #endregion
    }
}