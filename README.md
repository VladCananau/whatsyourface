# What's Your Face

[![Build Status](https://dev.azure.com/vladcananau/WhatsYourFace/_apis/build/status/VladCananau.whatsyourface?branchName=master)](https://dev.azure.com/vladcananau/WhatsYourFace/_build/latest?definitionId=1&branchName=master)

Play with it at https://whatsyourfacefrontend.azurewebsites.net/. Upload a photo of your face (it must contain no more and no less than one face) and see what name you really look like. I was, of course, insipired by conversations like: 

> "What's your name?" 
> "Vlad."
> "Oh really? You look more like a Hans." 

**What's Your Face** uses [Azure / Bing Cognitive Services](https://azure.microsoft.com/en-us/services/cognitive-services/face/) with a data set of ~4000 faces and first names. It looks for the faces that are most similar to the photo you uploaded and based on those similarities tells you the name(s) that your face was really meant for. Experiment with various photos, you'll get slightly different results. All in all, don't take it too seriously, it's just a bit of fun.

**Important**: The photos you upload are _not_ stored at all serverside. Cognitive Services works only on the features (sic of Machine Learning) it extracts from photos. In fact, not even the photos in my dataset are stored anywhere anymore. 

Future goals for this little pet project are to enlarge / improve the dataset and to evolve the underlying algorithm from the current strategy of matching against a set of individual faces to one that matches against the "average features" of faces having the same name. If anyone is interested in this kind of stuff I'm always up for a chat.
