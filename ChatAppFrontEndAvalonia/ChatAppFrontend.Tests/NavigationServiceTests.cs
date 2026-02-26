using ChatAppFrontEnd.Source.Services.Concrete;
using ChatAppFrontEnd.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ChatAppFrontend.Tests
{
    public class NavigationServiceTests
    {
        [Test]
        public void Navigate_SetsCurrentPanelAndCallsOnShow()
        {
            var service = CreateService(out var firstPanel, out _);

            service.Navigate<FirstPanelViewModel>();

            Assert.That(service.CurrentPanel, Is.SameAs(firstPanel));
            Assert.That(firstPanel.OnShowCount, Is.EqualTo(1));
        }

        [Test]
        public void GoBack_CallsOnHideAndRestoresPriorPanel()
        {
            var service = CreateService(out var firstPanel, out var secondPanel);
            service.Navigate<FirstPanelViewModel>();
            service.Navigate<SecondPanelViewModel>();

            service.GoBack();

            Assert.That(service.CurrentPanel, Is.SameAs(firstPanel));
            Assert.That(secondPanel.OnHideCount, Is.EqualTo(1));
            Assert.That(firstPanel.OnShowCount, Is.EqualTo(2));
        }

        [Test]
        public void ClearBackStack_LeavesOneItem()
        {
            var service = CreateService(out _, out var secondPanel);
            service.Navigate<FirstPanelViewModel>();
            service.Navigate<SecondPanelViewModel>();

            service.ClearBackStack();

            Assert.That(service.CurrentPanel, Is.SameAs(secondPanel));
            Assert.That(service.CanGoBack, Is.False);
        }

        private static NavigationService CreateService(out FirstPanelViewModel firstPanel, out SecondPanelViewModel secondPanel)
        {
            firstPanel = new FirstPanelViewModel();
            secondPanel = new SecondPanelViewModel();

            var provider = new ServiceCollection()
                .AddSingleton(firstPanel)
                .AddSingleton(secondPanel)
                .BuildServiceProvider();

            return new NavigationService(provider, new MasterWindowViewModel());
        }

        private class TestPanelViewModel : PanelViewModelBase
        {
            public int OnShowCount { get; private set; }
            public int OnHideCount { get; private set; }

            public override void OnShow() => OnShowCount++;
            public override void OnHide() => OnHideCount++;
        }

        private sealed class FirstPanelViewModel : TestPanelViewModel
        {
        }

        private sealed class SecondPanelViewModel : TestPanelViewModel
        {
        }
    }
}
