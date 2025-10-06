window.swEvents = {
    startListening: function () {
        if ('serviceWorker' in navigator) {
            navigator.serviceWorker.register('service-worker.js')
                .then(reg => {
                    // Listen for new SW installing
                    reg.addEventListener('updatefound', () => {
                        const installingWorker = reg.installing;
                        if (installingWorker) {
                            DotNet.invokeMethodAsync('GoldEx.Client.Components', 'OnServiceWorkerInstalling');
                        }
                    });

                    // Also listen for messages from the SW
                    navigator.serviceWorker.addEventListener('message', event => {
                        if (event.data?.type === 'NEW_VERSION_AVAILABLE') {
                            window.location.reload();
                        }
                    });
                });
        }
    }
};
