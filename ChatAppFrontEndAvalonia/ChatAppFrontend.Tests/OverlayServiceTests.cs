using ChatAppFrontEnd.Source.Services.Concrete;
using ChatAppFrontEnd.ViewModels;

namespace ChatAppFrontend.Tests
{
    public class OverlayServiceTests
    {
        [Test]
        public void ShowOverlay_SetsExpectedViewModelAndVisibility()
        {
            var masterWindow = new MasterWindowViewModel();
            var service = new OverlayService(masterWindow);
            var overlayViewModel = new TestOverlayViewModel();

            service.ShowOverlay(overlayViewModel, 12, 34, () => { });

            Assert.That(masterWindow.OverlayViewModel, Is.SameAs(overlayViewModel));
            Assert.That(masterWindow.OverlayViewModelCenter, Is.Null);
            Assert.That(masterWindow.OverlayOffsetTop, Is.EqualTo(12));
            Assert.That(masterWindow.OverlayOffsetLeft, Is.EqualTo(34));
            Assert.That(masterWindow.IsOverlayVisible, Is.True);
            Assert.That(masterWindow.IsOverlayCenterVisible, Is.False);
        }

        [Test]
        public void ShowOverlayCentered_TogglesCenteredVisibility()
        {
            var masterWindow = new MasterWindowViewModel();
            var service = new OverlayService(masterWindow);
            var centeredViewModel = new TestOverlayViewModel();

            service.ShowOverlayCentered(centeredViewModel, () => { });

            Assert.That(masterWindow.OverlayViewModel, Is.Null);
            Assert.That(masterWindow.OverlayViewModelCenter, Is.SameAs(centeredViewModel));
            Assert.That(masterWindow.IsOverlayVisible, Is.False);
            Assert.That(masterWindow.IsOverlayCenterVisible, Is.True);
        }

        [Test]
        public void HideOverlay_ClearsVisibility()
        {
            var masterWindow = new MasterWindowViewModel();
            var service = new OverlayService(masterWindow);
            service.ShowOverlay(new TestOverlayViewModel(), 1, 2, () => { });

            service.HideOverlay();

            Assert.That(masterWindow.OverlayViewModel, Is.Null);
            Assert.That(masterWindow.OverlayViewModelCenter, Is.Null);
            Assert.That(masterWindow.IsOverlayVisible, Is.False);
            Assert.That(masterWindow.IsOverlayCenterVisible, Is.False);
        }

        private sealed class TestOverlayViewModel : ViewModelBase
        {
        }
    }
}
