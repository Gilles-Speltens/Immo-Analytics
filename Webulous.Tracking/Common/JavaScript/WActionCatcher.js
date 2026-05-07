document.addEventListener('DOMContentLoaded', function () {

    // Sélectionne tous les éléments ayant une classe contenant "w-action--"
    const elements = document.querySelectorAll('[class*="w-action--"]');

    elements.forEach(function (el) {

        // Trouver la classe qui commence par "w-action--"
        const actionClass = Array.from(el.classList).find(cls => cls.startsWith('w-action--'));

        if (!actionClass) return;

        // Récupérer le nom après "--"
        const actionName = actionClass.split('--')[1];

        // Récupérer le href
        const href = el.getAttribute('href');

        // Ajouter un listener sans bloquer les autres comportements
        el.addEventListener('click', function () {

            fetch('/Home/ContactButton', {
                method: 'POST',
                body: JSON.stringify({
                    clientAction: actionName,
                    targetUrl: href
                }),
                headers: {
                    'Content-Type': 'application/json'
                }
            });
        }, false);
    });
});

//Généré par ChatGPT
document.addEventListener('DOMContentLoaded', function () {



    const elements = document.querySelectorAll('[class*="w-action--"]');



    elements.forEach(function (el) {



        const actionClass = Array.from(el.classList)

            .find(cls => cls.startsWith('w-action--'));



        if (!actionClass) {

            return;

        }



        const actionName = actionClass.split('--')[1];

        const href = el.getAttribute('href');



        el.addEventListener('click', async function () {



            console.log('WActionCatcher: Click detected');

            console.log('WActionCatcher: Action =', actionName);

            console.log('WActionCatcher: Target URL =', href);



            try {



                const pathParts = window.location.pathname

                    .split('/')

                    .filter(Boolean);



                const languages = ['fr', 'nl', 'en', 'de'];



                const langIndex = pathParts.findIndex(p =>

                    languages.includes(p.toLowerCase())

                );



                let basePath = '';



                if (langIndex >= 0) {



                    // Exemple :

                    // /F/MCQGFIEUTM/fr

                    // ou

                    // /fr

                    basePath = '/' + pathParts

                        .slice(0, langIndex + 1)

                        .join('/');



                }

                else if (

                    pathParts.length >= 2 &&

                    pathParts[0].toUpperCase() === 'F'

                ) {



                    // Exemple :

                    // /F/MCQGFIEUTM

                    basePath = '/' + pathParts[0] + '/' + pathParts[1];

                }



                const requestUrl = `${basePath}/Home/ContactButton`;



                console.log('WActionCatcher: Request URL =', requestUrl);



                const payload = {

                    clientAction: actionName,

                    targetUrl: href

                };



                console.log('WActionCatcher: Payload =', payload);



                const response = await fetch(requestUrl, {

                    method: 'POST',

                    keepalive: true,

                    headers: {

                        'Content-Type': 'application/json'

                    },

                    body: JSON.stringify(payload)

                });



                console.log('WActionCatcher: Response status =', response.status);

                console.log('WActionCatcher: Response ok =', response.ok);



                const responseText = await response.text();



                console.log('WActionCatcher: Response body =', responseText);



                if (!response.ok) {



                    console.error(

                        'WActionCatcher: Server returned an error',

                        {

                            status: response.status,

                            statusText: response.statusText,

                            response: responseText

                        }

                    );

                }

                else {



                    console.log('WActionCatcher: Request successful');

                }



            }

            catch (error) {



                console.error(

                    'WActionCatcher: Fetch failed',

                    error

                );

            }

        }, false);

    });

});