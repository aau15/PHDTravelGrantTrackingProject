<?php
/**
 * Created by PhpStorm.
 * User: Xavier
 * Date: 2016/10/17
 * Time: 8:54 pm
 */

$StudentID="";
$password=$password_confirm="";
$Email="";
$Name="";
if ($_SERVER["REQUEST_METHOD"] == "POST") {
    //Check StudentID
    if (empty($_POST["StudentID"])) {
        $nameErr = "Sorry, ID is required";
    } else {
        $name = test_input($_POST["StudentID"]);
        // Check name
        if (!preg_match("/^[a-zA-Z0—9 ] *$/", $name)) {
            $nameErr = "Sorry, only letters ,numbers and space are allowed";
        }
    }
    //Check Name
    if (empty($_POST["Name"])) {
        $emailErr = "Sorry, Name is required";
    } else {
        $email = test_input($_POST["Email"]);
    }
    //Check Email
    if (empty($_POST["Email"])) {
        $emailErr = "Sorry, email is required";
    } else {
        $email = test_input($_POST["Email"]);
        // Check email
        if (!preg_match("/([\w\-]+\@[\w\-]+\.[\w\-]+)/",$email)) {
            $emailErr = "Invalid email format";
        }
    }
    //Check PassWord
    if (empty($_POST["PassWord"])) {
        $passwordErr = "Sorry, password is required";
    } else {
        $password = test_input($_POST["PassWord"]);
    }
    if (empty($_POST["PassWord_confirm"])) {
        $psw_confirmErr = "Sorry, PassWord confirm is required";
    } else {
        $psw_confirm = test_input($_POST["PassWord_confirm"]);
    }
}
function test_input($data) {
    $data = trim($data);
    $data = stripslashes($data);
    $data = htmlspecialchars($data);
    return $data;
}

if(isset($_POST["Submit"]) && $_POST["Submit"] == "Sign up")
{
    $StudentID = $_POST["StudentID"];
    $password = $_POST["PassWord"];
    $password_confirm= $_POST["PassWord_confirm"];
    $Name = $_POST["Name"];
    $Email = $_POST["Email"];
    if($StudentID == "" || $password == "" || $password_confirm == "" ||$Name == "" || $Email == ""  )
    {
        echo "<script>alert('Please check whether your registration is complete'); history.go(-1);</script>";
    }
    else {
        if ($password == $password_confirm) {

            //Connect Database Server
            $conn = new mysqli("mysql5006.smarterasp.net", "a117c6_phd", "phd12345");
            if ($conn->connect_error) {
                die("Failed " . $conn->connect_error);
            }
            //SQL query
            $sql = "SELECT StudentID FROM db_a117c6_phd.student WHERE StudentID = '$_POST[StudentID]'";
            //Conduct SQL query
            $result = $conn->query($sql);
            $num = mysqli_num_rows($result);
            if($num )// ID exists
            {
                echo "<script>alert('Sorry, it looks like the StudentID belongs to an existing account.'); history.go(-1);</script>";
            }
            else//ID does not exist
            {
                $sql_insert = "INSERT INTO db_a117c6_phd.student (StudentID,Password) VALUES ('$_POST[StudentID]','$_POST[password]')";
                $insert_result = $conn->query($sql_insert);
                if($insert_result )
                {
                    echo "<script>alert('Sign up succeeded!'); window.location='index.php';</script>";
                }
                else
                {
                    echo "<script>alert('System is busy! Please try again later!'); history.go(-1);</script>";
                }
            }

        }
        else
        {
            echo "<script>alert('Password and Re-entered password do not match!'); history.go(-1);</script>";
        }
    }
}
else
{
    echo "<script>alert('Submit Failed'); history.go(-1);</script>";
}

$conn->close();

?>