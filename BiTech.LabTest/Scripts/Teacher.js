// Define the `phonecatApp` module
var labTest = angular.module('LabTestApp', []);

// Define the `PhoneListController` controller on the `phonecatApp` module
labTest.controller('StudentTrackingController', function PhoneListController($scope) {
    $scope.students = [];
    // Số lượng thí sinh đã vào phòng thi
    $scope.statistics = [];
   

    // Declare a proxy to reference the hub.
    var chat = $.connection.testHub;

    // Nhận thống kê Online sau đi đã request
    chat.client.teacherRetrieveOnlineStatistics = function (response) {
        $scope.students                      = response;
        $scope.statistics.totalOnlineStudent = $scope.students.length;

        // Số lượng các học sinh theo từng trạng thái
        $scope.statistics.totalWaitingStudent   = 0;
        $scope.statistics.totalDoingTestStudent = 0;
        $scope.statistics.totalFinishedStudent  = 0;

        // Số lượng thí sinh theo từng loại điểm
        $scope.statistics.totalScore0 = 0;
        $scope.statistics.totalScore1 = 0;
        $scope.statistics.totalScore2 = 0;
        $scope.statistics.totalScore3 = 0;
        $scope.statistics.totalScore4 = 0;
        $scope.statistics.totalScore5 = 0;
        $scope.statistics.totalScore6 = 0;
        $scope.statistics.totalScore7 = 0;
        $scope.statistics.totalScore8 = 0;
        $scope.statistics.totalScore9 = 0;
        $scope.statistics.totalScore10 = 0;

        // Độ rộng hiển thị tỉ lệ giữa các nhóm
        $scope.statistics.totalWaitingStudentPercent   = 0;
        $scope.statistics.totalDoingTestStudentPercent = 0;
        $scope.statistics.totalFinishedStudentPercent  = 0;

        var totalWaiting   = 0;
        var totalDoingTest = 0;
        var totalFinished  = 0;

        // Đếm số lượng Online theo từng nhóm
        for (var i = 0; i < $scope.students.length; i++) {
            switch ($scope.students[i].TestStep) {
                case 0:
                    totalWaiting += 1;
                    break;
                case 1:
                    totalDoingTest += 1;
                    break;
                case 2:
                    totalFinished += 1;
                    break;
                default:
            }

            var mark = $scope.students[i].Mark;
            switch (true) {
                case (mark == ""):
                    break;
                case (mark <= 1):
                    $scope.statistics.totalScore0 += 1;
                    break;
                case (mark <=2):
                    $scope.statistics.totalScore1 += 1;
                    break;
                case (mark <=3):
                    $scope.statistics.totalScore2 += 1;
                    break;
                case (mark <=4):
                    $scope.statistics.totalScore3 += 1;
                    break;
                case (mark < 5):
                    $scope.statistics.totalScore4 += 1;
                    break;
                case (mark < 6):
                    $scope.statistics.totalScore5 += 1;
                    break;
                case (mark < 7):
                    $scope.statistics.totalScore6 += 1;
                    break;
                case (mark < 8):
                    $scope.statistics.totalScore7 += 1;
                    break;
                case (mark < 9):
                    $scope.statistics.totalScore8 += 1;
                    break;
                case (mark < 10):
                    $scope.statistics.totalScore9 += 1;
                    break;
                case (mark == 10):
                    $scope.statistics.totalScore10 += 1;
                    break;

            }
        }


        $scope.statistics.totalWaitingStudent   = totalWaiting;
        $scope.statistics.totalDoingTestStudent = totalDoingTest;
        $scope.statistics.totalFinishedStudent  = totalFinished;

        $scope.statistics.totalWaitingStudentPercent   = (totalWaiting * 100) / $scope.statistics.totalOnlineStudent;
        $scope.statistics.totalDoingTestStudentPercent = (totalDoingTest * 100) / $scope.statistics.totalOnlineStudent;
        $scope.statistics.totalFinishedStudentPercent  = (totalFinished * 100) / $scope.statistics.totalOnlineStudent;

        $scope.$apply();
    }

    // Start the connection.
    $.connection.hub.start().done(function () {
        var getOnlineStatistics = setInterval(function () {

            var testIdElement = document.getElementById("TestId");
            var testId        = (testIdElement != null) ? testIdElement.value : "";

            // Request đẻ lấy thống kê Online
            chat.server.teacherRequestOnlineStatistics(testId);
        }, 1000);

    });
});

