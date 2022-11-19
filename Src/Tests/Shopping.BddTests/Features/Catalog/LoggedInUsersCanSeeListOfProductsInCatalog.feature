Feature: Logged in users can see list of products in catalog
	As a user 
	I want to be able to see list of products in a catalog

Scenario: Guest users should not see catalog
	Given a user that is not logged in
	When tries to get catalog
	Then unathorized response is returned

Scenario: Logged in users should see catalog
	Given a logged in user
	When tries to get products from catalog
	Then catalog with products is returned