# FindTopRatedPlaces
This is an application to find the highest rated places (created for restaurants) in an area using google's ratings.

I was very dissapointed with google's system for finding places nearby. Specifically, I had troubles finding good restaurants nearby. The issue is that google does not provide an option to sort restaurants (or places) in a city or area by ratings. This application will find all places within the specified area around the given location of the given type and sort them by highest ratings and output all relevant information to these places to a csv file.

Some use cases:
 - Finding the best local restaurants to eat at
 - Finding a nice movie theatre to see a movie
 - Finding nice lodging, parks, amusement parks, etc.

How to use the application:
1. Get an api key from google by linking a billing account:
  https://developers.google.com/maps/documentation/places/web-service/cloud-setup
  https://console.cloud.google.com/google/maps-apis/start
  NOTE: You will not be charged if you are on a free trial and google provides a 200$ credit each month which correlates to roughly 12,000 api calls. If you are worried about getting charged, you can setup limits in google's api and you can set an api limit in this application when launching it to ensure that there is no chance of being charged.

