<<<<<<< HEAD

<html>
<body>
<?php
$servername = "mysql5006.smarterasp.net";
$username = "a117c6_phd";
$password = "phd12345";


@session_start();
$security_token = $_SESSION['security_token'] = uniqid(rand());



if(! isset($_POST["Submit"]) ){
$Id = $_POST["Id"];
$passWord = $_POST["passWord"];
    if($Id == "" || $passWord == "") {
        echo "<script>alert('Please enter ID or password'); history.go(-1);</script>";
    }
    else{
        $conn = new mysqli($servername, $username, $password);
        if ($conn->connect_error) {
            die("Failed " . $conn->connect_error);
        }
        $sql = "SELECT StudentID FROM db_a117c6_phd.student WHERE StudentID = '$_POST[Id]' and PassWord = '$_POST[passWord]' ";
        $result = $conn->query($sql);
        $num = mysqli_num_rows($result);


        if ($num ) {
//            $_SESSION["StudentID"]=$_POST["Id"];
//            $sql2 = "SELECT StudentID  FROM db_a117c6_phd.student WHERE StudentID = '$_SESSION[StudentID]'";
//            $result = $conn->query($sql2);
//            $t_result = mysqli_fetch_array($result);

//            if($result==0 || $t_result==0){
//                echo "<script>alert('Incorrect email or password!');history.go(-1);</script>";
//            }
//            else{
//                $_SESSION["StudentID"]=$t_result[0];
//                $_SESSION["PassWord"]=$t_result[1];
                echo "<script>window.location='index.html';</script>";
//            }
        }
        else {
            echo "<script>alert('Incorrect ID or password!');history.go(-1);</script>";
        }

        $conn->close();
    }}

?>


</body>
</html>

=======

<html>
<body>
<?php
$servername = "mysql5006.smarterasp.net";
$username = "a117c6_phd";
$password = "phd12345";


@session_start();
$security_token = $_SESSION['security_token'] = uniqid(rand());



if(! isset($_POST["Submit"]) ){
	$Id = $_POST["Id"];
	$passWord = $_POST["passWord"];
	if($Id == "" || $passWord == "") {
		echo "<script>alert('Please enter ID or password'); history.go(-1);</script>";
	}
	else{
		$conn = new mysqli($servername, $username, $password);
		if ($conn->connect_error) {
			die("Failed " . $conn->connect_error);
		}
		$sql = "SELECT StudentID FROM db_a117c6_phd.student WHERE StudentID = '$_POST[Id]' and PassWord = '$_POST[passWord]' ";
		$result = $conn->query($sql);
		$num = mysqli_num_rows($result);
		
		//////////////////////////////////////////////////
		$_SESSION["StudentID"]=$_POST["Id"];
		$currentTime = md5(date('y-m-d h:i:s',time()));
		$_SESSION["tripID"]=$currentTime + $_POST["Id"];
		//var_dump($_SESSION["tripID"], $currentTime, date('y-m-d h:i:s',time()));
		/////////////////////////////////////////////////
		
		if ($num ) {
			//            $_SESSION["StudentID"]=$_POST["Id"];
			//            $sql2 = "SELECT StudentID  FROM db_a117c6_phd.student WHERE StudentID = '$_SESSION[StudentID]'";
			//            $result = $conn->query($sql2);
			//            $t_result = mysqli_fetch_array($result);

			//            if($result==0 || $t_result==0){
			//                echo "<script>alert('Incorrect email or password!');history.go(-1);</script>";
			//            }
			//            else{
			//                $_SESSION["StudentID"]=$t_result[0];
			//                $_SESSION["PassWord"]=$t_result[1];
			
			
			echo "<script>window.location='stuDetailsTable.html';</script>";
			//            }
		}
		else {
			echo "<script>alert('Incorrect email or password!');history.go(-1);</script>";
		}


		$conn->close();
	}}

	?>


</body>
</html>

>>>>>>> 438a29eeea52ad17ca489ec548131f7d22c8de3c
