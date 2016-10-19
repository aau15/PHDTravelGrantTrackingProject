<?php

session_cache_limiter('private, must-revalidate');
@session_start();
$security_token = $_SESSION['security_token'] = uniqid(rand());
if (isset($_POST['submit'])){
	/* $conn = new mysqli('mysql5006.smarterasp.net', 'a117c6_phd', 'phd12345', 'db_a117c6_phd');
	if ($conn -> connect_error) {
		die('Could not connect: ' . $conn -> connect_error);
	}else {		 */
		
		if (empty($_POST['purpose'])){
			$type = 0;
			$cName = $_POST['cName'];
			$cURL = $_POST['cURL'];
			$city = $_POST['city'];
			$country = $_POST['country'];
			$ptitle = $_POST['ptitle'];
			$authors = $_POST['authors'];
			$purpose = "none";
		}
		else{
			$type = 1;
			$cName = "none";
			$cURL = "none";
			$city = "none";
			$country = "none";
			$ptitle = "none";
			$authors = "none";
			$purpose = $_POST['purpose'];
		}
		$_SESSION["type"]=$type;
		$_SESSION["cName"]=$cName;
		$_SESSION["cURL"]=$cURL;
		$_SESSION["city"]=$city;
		$_SESSION["country"]=$country;
		$_SESSION["ptitle"]=$ptitle;
		$_SESSION["authors"]=$authors;
		$_SESSION["purpose"]=$purpose;
		$status = 0;
		$_SESSION["status"]=$status;
	
		/* $query = "insert into trip
		(TripID, ConferenceName, ConferenceURL, City, Country, 
		PaperTitle, Author, Purpose, Type, Status, CostOfTrip)
		values('{$_SESSION["tripID"]}',
		'{$cName}','{$cURL}','{$city}','{$country}','{$ptitle}',
		'{$authors}','{$purpose}','{$type}','{$status}',1)";

		$result=$conn->query($query); */
		
<<<<<<< HEAD
		echo "<script>window.location='preTripTable.html';</script>";
=======
		echo "<script>window.location='preTripTable.php';</script>";
>>>>>>> 1dacf54ec6af74c955fdc15477f459a6a99dab8a
		//$conn->close();

	//}
}