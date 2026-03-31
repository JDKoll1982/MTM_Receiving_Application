-- phpMyAdmin SQL Dump
-- version 5.2.2
-- https://www.phpmyadmin.net/
--
-- Host: localhost:3306
-- Generation Time: Mar 27, 2026 at 09:52 AM
-- Server version: 5.7.24
-- PHP Version: 8.3.1

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";

--
-- Dumping data for table `receiving_history`
--

INSERT IGNORE INTO `receiving_history` (`id`, `load_guid`, `quantity`, `part_id`, `po_number`, `po_line_number`, `employee_number`, `heat`, `transaction_date`, `initial_location`, `coils_on_skid`, `label_number`, `vendor_name`, `part_description`, `is_non_po_item`, `part_skid_sequence`, `part_skid_total`, `created_at`) VALUES
(8, 'feed9255-ab98-4204-9cfb-e827b66dd469', 100, '78835831', '068026', NULL, 6229, '96788', '2026-03-24', 'Nothing Entered', 0, 1, 'Atlantic Gasket Corporation', 'Seal Cover Plate Firewall', 0, 1, 1, '2026-03-25 18:40:08'),
(9, 'a7ce1466-b4b9-4aff-a52a-320e0023a731', 2000, '23-10721-100', '067868', NULL, 6229, 'C63731', '2026-03-24', 'RECV DESK', NULL, 1, 'Buckeye Fasteners, Inc', 'Stud-Pjtn Weld, 1/4-20 Thd 1.00\" Lg', 0, 1, 1, '2026-03-25 18:40:08'),
(10, '8025d177-1393-43aa-b0b9-c90456073141', 1, 'MMF0000250', '068694', NULL, 6229, 'None', '2026-03-24', 'RECV DESK', NULL, 1, 'McMaster-Carr Supply Co', 'Blank, .250 X 15.000 X 15.000', 0, 1, 1, '2026-03-25 18:40:08'),
(11, '4e405b1d-5585-40b2-aaf4-b4a2b21fbe5f', 144, 'RCA05', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 1, NULL, 'Tote (Pool) - 12.00 x 7.00 x 5.00', 1, 1, 2, '2026-03-25 18:40:08'),
(12, '3c3a82d4-01f2-4e7c-bc89-e60ad78fb75c', 144, 'RCA05', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 2, NULL, 'Tote (Pool) - 12.00 x 7.00 x 5.00', 1, 2, 2, '2026-03-25 18:40:08'),
(13, '27bdc81d-427d-4998-92a1-5abd734203f6', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 1, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 1, 24, '2026-03-25 18:40:08'),
(14, 'd2751919-e905-425c-b934-634eadfc6dec', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 2, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 2, 24, '2026-03-25 18:40:08'),
(15, 'c9122700-ff97-453d-b7b7-c154257ebadc', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 3, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 3, 24, '2026-03-25 18:40:08'),
(16, '26b8bf32-cfa6-4e0a-a433-3e08c2924462', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 4, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 4, 24, '2026-03-25 18:40:08'),
(17, '956324d3-eb8a-4c07-acd9-ba5fbb4b4c70', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 5, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 5, 24, '2026-03-25 18:40:08'),
(18, '281d4f53-ef9a-4c06-9600-8b3ec4e64d17', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 6, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 6, 24, '2026-03-25 18:40:08'),
(19, 'c00fde17-80ac-429f-82a8-146e2b63bd09', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 7, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 7, 24, '2026-03-25 18:40:08'),
(20, '69fd860f-70fc-45c3-8066-109ede227ec4', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 8, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 8, 24, '2026-03-25 18:40:08'),
(21, '1e36f7a7-3edc-4233-9981-e6bef087852b', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 9, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 9, 24, '2026-03-25 18:40:08'),
(22, '8bdeb7aa-5738-48df-973a-5d8936c07d22', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 10, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 10, 24, '2026-03-25 18:40:08'),
(23, '72f0dcaf-02e8-4856-82cf-af57f1ebc97f', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 11, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 11, 24, '2026-03-25 18:40:08'),
(24, '94cd31d7-43ab-42bc-9f4a-45d3eae1579b', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 12, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 12, 24, '2026-03-25 18:40:08'),
(25, '3c1ea186-8d2a-4fb3-93ea-6718236019ac', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 13, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 13, 24, '2026-03-25 18:40:08'),
(26, '453548e4-c4e3-452e-b258-97e0436f8ab4', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 14, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 14, 24, '2026-03-25 18:40:08'),
(27, 'd54f517a-bf64-4ba6-acef-eca159c95b61', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 15, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 15, 24, '2026-03-25 18:40:08'),
(28, 'b5d60ddd-a673-4ba0-bb88-b88650829dd3', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 16, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 16, 24, '2026-03-25 18:40:08'),
(29, '8edf6865-925d-4dd7-af3f-6277f3bef26a', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 17, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 17, 24, '2026-03-25 18:40:08'),
(30, 'b3770ddb-45b9-4602-9aea-819d4d1bc124', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 18, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 18, 24, '2026-03-25 18:40:08'),
(31, 'ed873b40-5338-4abb-9f00-1859236a2dd6', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 19, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 19, 24, '2026-03-25 18:40:08'),
(32, '74b53ce3-d867-46d0-81ae-53d979c99b11', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 20, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 20, 24, '2026-03-25 18:40:08'),
(33, '5af400ce-e370-4a94-9f78-44580fd366db', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 21, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 21, 24, '2026-03-25 18:40:08'),
(34, 'a34a553f-5a40-42c8-9c2a-f432951f75e9', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 22, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 22, 24, '2026-03-25 18:40:08'),
(35, 'f137191c-440c-411e-afce-5d33d3aacf39', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 23, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 23, 24, '2026-03-25 18:40:08'),
(36, 'f8a01015-7926-46f1-863e-397e33d97e0e', 1, 'RCG25', NULL, NULL, 6229, 'N/A', '2026-03-24', 'RECV', NULL, 24, NULL, 'Tote (Pool) - 32.00 x 30.00 x 25.00', 1, 24, 24, '2026-03-25 18:40:08'),
(37, '365f54ff-766d-4c57-af01-508d7453b95c', 6750, '23-14200-045', '067306', NULL, 6229, 'Assorted', '2026-03-24', 'RECV', NULL, 1, 'Facil North America Inc', 'Bolt, Rnd Hd, Rib Neck, M12x45', 0, 1, 3, '2026-03-25 18:40:08'),
(38, '48a94d28-d1f6-4289-9058-66c541f7b17d', 6750, '23-14200-045', '067306', NULL, 6229, 'Assorted', '2026-03-24', 'RECV', NULL, 2, 'Facil North America Inc', 'Bolt, Rnd Hd, Rib Neck, M12x45', 0, 2, 3, '2026-03-25 18:40:08'),
(39, '6d08e531-b1d0-4f3c-afd2-993d7b759df2', 6500, '23-14200-045', '067306', NULL, 6229, 'Assorted', '2026-03-24', 'RECV', NULL, 3, 'Facil North America Inc', 'Bolt, Rnd Hd, Rib Neck, M12x45', 0, 3, 3, '2026-03-25 18:40:08'),
(40, '2fb7acaf-fbfb-4688-b1d5-9e56dd306872', 6000, '23-12742-005', '066285', NULL, 6229, 'N2502K0011', '2026-03-24', 'RECV DESK', NULL, 1, 'Facil North America Inc', 'Nut, Clinch, Lkg, M5 x 0.8', 0, 1, 3, '2026-03-25 18:40:08'),
(41, 'e23060d3-e5d7-4c97-9d79-38723f220eab', 6000, '23-12742-005', '066285', NULL, 6229, 'N2502K0011', '2026-03-24', 'RECV DESK', NULL, 2, 'Facil North America Inc', 'Nut, Clinch, Lkg, M5 x 0.8', 0, 2, 3, '2026-03-25 18:40:08'),
(42, '0e2a9275-db20-486d-852c-75e2225c03ce', 6000, '23-12742-005', '066285', NULL, 6229, 'N2502K0011', '2026-03-24', 'RECV DESK', NULL, 3, 'Facil North America Inc', 'Nut, Clinch, Lkg, M5 x 0.8', 0, 3, 3, '2026-03-25 18:40:08'),
(43, 'b6023859-ba6d-4aef-b2d6-24b36c846b52', 2500, '23-10721-125', '067306', NULL, 6229, 'FI29982', '2026-03-24', 'RECV DESK', NULL, 1, 'Facil North America Inc', 'Stud-Pjtn Weld, 1/4-20 Thd 1.25\" Lg', 0, 1, 2, '2026-03-25 18:40:08'),
(44, '8af3bf26-3a7e-4a60-bd13-dfe9fcd6cb75', 200, '23-10721-125', '067306', NULL, 6229, 'FI29982', '2026-03-24', 'RECV DESK', NULL, 2, 'Facil North America Inc', 'Stud-Pjtn Weld, 1/4-20 Thd 1.25\" Lg', 0, 2, 2, '2026-03-25 18:40:08'),
(71, '81c285f7-8afe-4bd7-a48e-66fb4bd6a383', 2452, 'MMF0005416', '068539', NULL, 6229, '831P03550', '2026-03-25', 'RECV', NULL, 1, 'Horizon Steel Company', 'Sheet, 16Ga (.059) X 60.000 X 120.000', 0, 1, 1, '2026-03-26 12:11:21'),
(72, '921a6b35-9ded-41a5-8b9b-b0f599e41c2b', 4170, 'MMC0000388', '066865', NULL, 6229, '42603010', '2026-03-25', 'V-E0-01', NULL, 1, 'MST Steel Corporation', 'Coil, 12Ga X 11.750', 0, 1, 4, '2026-03-26 12:11:21'),
(73, '03e7a83d-de74-4ae5-803b-003fb876e23b', 4160, 'MMC0000388', '066865', NULL, 6229, '42603010', '2026-03-25', 'V-E0-01', NULL, 2, 'MST Steel Corporation', 'Coil, 12Ga X 11.750', 0, 2, 4, '2026-03-26 12:11:21'),
(74, '2cc91186-c690-4b19-9ef5-f79b0b6f7755', 4120, 'MMC0000388', '066865', NULL, 6229, '42603010', '2026-03-25', 'V-E0-01', NULL, 3, 'MST Steel Corporation', 'Coil, 12Ga X 11.750', 0, 3, 4, '2026-03-26 12:11:21'),
(75, '3d026d00-0800-44cc-a3cc-568eabe9ca8d', 4170, 'MMC0000388', '066865', NULL, 6229, '42603010', '2026-03-25', 'V-E0-01', NULL, 4, 'MST Steel Corporation', 'Coil, 12Ga X 11.750', 0, 4, 4, '2026-03-26 12:11:21'),
(76, 'c46417f4-8d5b-4bf6-ad34-76a4f6ac37c1', 500, '32155-7900', '067244', NULL, 6229, 'Nothing Entered', '2026-03-25', 'RECV', NULL, 1, 'National Metalwares, L.P.', 'Pipe', 0, 1, 2, '2026-03-26 12:11:21'),
(77, 'b5b880dd-2f10-484d-bf12-e3e67264efc1', 520, '32155-7900', '067244', NULL, 6229, 'Nothing Entered', '2026-03-25', 'RECV', NULL, 2, 'National Metalwares, L.P.', 'Pipe', 0, 2, 2, '2026-03-26 12:11:21'),
(78, '68c43bbc-a75b-4a43-b7ad-a5cd3fdaae8d', 1000, '32155-7959', '067244', NULL, 6229, 'Nothing Entered', '2026-03-25', 'RECV', NULL, 1, 'National Metalwares, L.P.', 'Pipe', 0, 1, 1, '2026-03-26 12:11:21'),
(79, 'ef70fe68-882e-4cdb-9df4-62157b0d3a42', 1000, '32155-7960', '067244', NULL, 6229, 'Nothing Entered', '2026-03-25', 'RECV', NULL, 1, 'National Metalwares, L.P.', 'Pipe', 0, 1, 1, '2026-03-26 12:11:21'),
(80, '1910eabc-34db-4046-96fd-74e2a291054e', 5630, 'MMC0000650', '068241', NULL, 6229, '33608', '2026-03-25', 'V-F0-04', NULL, 1, 'Skana Aluminum Company', 'Coil, .054 X 40.237', 0, 1, 6, '2026-03-26 12:11:21'),
(81, '0af87d77-ad9b-447e-bc88-978cdb596e5f', 4186, 'MMC0000650', '068241', NULL, 6229, '33607', '2026-03-25', 'V-F0-04', NULL, 2, 'Skana Aluminum Company', 'Coil, .054 X 40.237', 0, 2, 6, '2026-03-26 12:11:21'),
(82, '67ef9768-3805-4202-ac7f-3222a6297106', 4350, 'MMC0000650', '068241', NULL, 6229, '33607', '2026-03-25', 'V-F0-04', NULL, 3, 'Skana Aluminum Company', 'Coil, .054 X 40.237', 0, 3, 6, '2026-03-26 12:11:21'),
(83, '1bfe64bc-cdc9-448e-a491-db8d7c8651cc', 3850, 'MMC0000650', '068241', NULL, 6229, '33607', '2026-03-25', 'V-F0-04', NULL, 4, 'Skana Aluminum Company', 'Coil, .054 X 40.237', 0, 4, 6, '2026-03-26 12:11:21'),
(84, 'a650a94f-4467-46f9-8e77-3fdd3edb4286', 4064, 'MMC0000650', '068241', NULL, 6229, '33607', '2026-03-25', 'V-F0-04', NULL, 5, 'Skana Aluminum Company', 'Coil, .054 X 40.237', 0, 5, 6, '2026-03-26 12:11:21'),
(85, '951d318f-9d15-404f-a339-672b99dc9cf2', 4346, 'MMC0000650', '068241', NULL, 6229, '33607', '2026-03-25', 'V-F0-04', NULL, 6, 'Skana Aluminum Company', 'Coil, .054 X 40.237', 0, 6, 6, '2026-03-26 12:11:21'),
(86, '91970534-0128-41a0-816a-f93455bcd1e7', 200, '975569', '068575', NULL, 6229, 'Nothing Entered', '2026-03-25', 'RECV DESK', NULL, 1, 'Buckeye Fasteners, Inc', 'Weld Screw M6 x 16', 0, 1, 1, '2026-03-26 12:11:21'),
(87, '8a1271f3-fdf1-4f97-8203-6ef95610361b', 1500, '963094', '067273', NULL, 6229, '192357', '2026-03-25', 'RECV desk', NULL, 1, 'Screw Industries ', 'Nut, Weld M10 * 7.9', 0, 1, 2, '2026-03-26 12:11:21'),
(88, '6efbe54a-0f70-4c2b-a5f1-d63c8699a7b6', 700, '963094', '067273', NULL, 6229, '192357', '2026-03-25', 'RECV desk', NULL, 2, 'Screw Industries ', 'Nut, Weld M10 * 7.9', 0, 2, 2, '2026-03-26 12:11:21'),
(102, 'ac225dba-332d-44b2-96c6-188eb1c699f2', 5230, 'MMC0000565', '066710', NULL, 6229, '559329', '2026-03-26', 'RECV', NULL, 1, 'Arlington Metals Corporation', 'Coil, 18Ga X 23.270', 0, 1, 8, '2026-03-26 13:43:33'),
(103, '781034e3-44d6-43a5-b26b-53612072aa15', 5270, 'MMC0000565', '066710', NULL, 6229, '559329', '2026-03-26', 'RECV', NULL, 2, 'Arlington Metals Corporation', 'Coil, 18Ga X 23.270', 0, 2, 8, '2026-03-26 13:43:33'),
(104, 'f6ca4065-c078-4cb0-b805-d37e508b3820', 5270, 'MMC0000565', '066710', NULL, 6229, '559329', '2026-03-26', 'RECV', NULL, 3, 'Arlington Metals Corporation', 'Coil, 18Ga X 23.270', 0, 3, 8, '2026-03-26 13:43:33'),
(105, '80b2bbad-8dff-4d2b-a6ab-d216bbd61ecd', 5230, 'MMC0000565', '066710', NULL, 6229, '559329', '2026-03-26', 'RECV', NULL, 4, 'Arlington Metals Corporation', 'Coil, 18Ga X 23.270', 0, 4, 8, '2026-03-26 13:43:33'),
(106, '9a849d17-aefa-428e-b8b8-6fd5f3e7d624', 5190, 'MMC0000565', '066710', NULL, 6229, '559329', '2026-03-26', 'RECV', NULL, 5, 'Arlington Metals Corporation', 'Coil, 18Ga X 23.270', 0, 5, 8, '2026-03-26 13:43:33'),
(107, '21151874-e686-4220-83ae-4041fd0eba76', 5190, 'MMC0000565', '066710', NULL, 6229, '559329', '2026-03-26', 'RECV', NULL, 6, 'Arlington Metals Corporation', 'Coil, 18Ga X 23.270', 0, 6, 8, '2026-03-26 13:43:33'),
(108, '29b8cf3d-4d4e-4cf8-bf41-f85d671cf1cb', 5240, 'MMC0000565', '066710', NULL, 6229, '559329', '2026-03-26', 'RECV', NULL, 7, 'Arlington Metals Corporation', 'Coil, 18Ga X 23.270', 0, 7, 8, '2026-03-26 13:43:33'),
(109, '534cf062-2e24-4a75-b671-a90ce6b47ffc', 5240, 'MMC0000565', '066710', NULL, 6229, '559329', '2026-03-26', 'RECV', NULL, 8, 'Arlington Metals Corporation', 'Coil, 18Ga X 23.270', 0, 8, 8, '2026-03-26 13:43:33'),
(110, '55c56538-5ae4-45c5-932c-bca8ebe1a492', 5445, 'MMC0000880', '068533', NULL, 6229, '335974', '2026-03-26', 'RECV', NULL, 1, 'Thyssenkrupp Materials NA', 'Coil, .250 X 12.500', 0, 1, 6, '2026-03-26 13:43:33'),
(111, '4c21cc5a-5f57-4370-a77b-4332a692d7b2', 5115, 'MMC0000880', '068533', NULL, 6229, '335974', '2026-03-26', 'RECV', NULL, 2, 'Thyssenkrupp Materials NA', 'Coil, .250 X 12.500', 0, 2, 6, '2026-03-26 13:43:33'),
(112, 'e14a9039-9686-4384-a76c-eff37787bf52', 5120, 'MMC0000880', '068533', NULL, 6229, '335974', '2026-03-26', 'RECV', NULL, 3, 'Thyssenkrupp Materials NA', 'Coil, .250 X 12.500', 0, 3, 6, '2026-03-26 13:43:33'),
(113, 'aee95e70-ddb0-42ab-843b-bf64f983e1de', 5130, 'MMC0000880', '068533', NULL, 6229, '335974', '2026-03-26', 'RECV', NULL, 4, 'Thyssenkrupp Materials NA', 'Coil, .250 X 12.500', 0, 4, 6, '2026-03-26 13:43:33'),
(114, 'adc57c11-1b28-4d4a-80cf-05f332165a12', 5130, 'MMC0000880', '068533', NULL, 6229, '335974', '2026-03-26', 'RECV', NULL, 5, 'Thyssenkrupp Materials NA', 'Coil, .250 X 12.500', 0, 5, 6, '2026-03-26 13:43:33'),
(115, '84e686f1-6216-446c-b10d-3d6828d6077b', 8425, 'MMC0000880', '068533', NULL, 6229, 'A2552140', '2026-03-26', 'RECV', NULL, 6, 'Thyssenkrupp Materials NA', 'Coil, .250 X 12.500', 0, 6, 6, '2026-03-26 13:43:33'),
(116, '982ac18c-44f8-45c0-b3d6-94e2d3545f06', 8825, 'MMC0000880', '068533', NULL, 6229, 'A2552140', '2026-03-26', 'RECV', NULL, 1, 'Thyssenkrupp Materials NA', 'Coil, .250 X 12.500', 0, 1, 6, '2026-03-26 13:43:33'),
(117, 'ff2b235e-65cb-4cf8-9cce-31db0ba6ee4d', 8845, 'MMC0000880', '068533', NULL, 6229, 'A2552140', '2026-03-26', 'RECV', NULL, 2, 'Thyssenkrupp Materials NA', 'Coil, .250 X 12.500', 0, 2, 6, '2026-03-26 13:43:33'),
(118, 'e81aa4b9-d9f6-4c50-8d53-d99312ebebb1', 8860, 'MMC0000880', '068533', NULL, 6229, 'A2552140', '2026-03-26', 'RECV', NULL, 3, 'Thyssenkrupp Materials NA', 'Coil, .250 X 12.500', 0, 3, 6, '2026-03-26 13:43:33'),
(119, '396f3305-17bc-41fe-91e4-ad74691d02b1', 5445, 'MMC0000880', '068533', NULL, 6229, '335974', '2026-03-26', 'RECV', NULL, 4, 'Thyssenkrupp Materials NA', 'Coil, .250 X 12.500', 0, 4, 6, '2026-03-26 13:43:33'),
(120, '2a29887d-23ed-403d-a5f5-469631bdeebc', 5445, 'MMC0000880', '068533', NULL, 6229, '335974', '2026-03-26', 'RECV', NULL, 5, 'Thyssenkrupp Materials NA', 'Coil, .250 X 12.500', 0, 5, 6, '2026-03-26 13:43:33'),
(121, 'd7c25d4b-340e-499d-82a2-1df7daa6a9db', 5450, 'MMC0000880', '068533', NULL, 6229, '335974', '2026-03-26', 'RECV', NULL, 6, 'Thyssenkrupp Materials NA', 'Coil, .250 X 12.500', 0, 6, 6, '2026-03-26 13:43:33'),
(122, '9b6a7882-a1c8-4dbd-8383-57f0fc063aa9', 2350, 'MMC0000570', '068494', NULL, 6229, 'F00844', '2026-03-26', 'RECV', NULL, 1, 'Dalco Metals, Inc.', 'Coil, 18Ga X 8.880', 0, 1, 1, '2026-03-26 13:43:33'),
(123, '75dc3f19-1896-4d01-a63d-70eb834ae346', 1320, 'MMC0000541', '068355', NULL, 6229, '833S63500', '2026-03-26', 'RECV', NULL, 1, 'Dalco Metals, Inc.', 'Coil, 11Ga X 6.750', 0, 1, 1, '2026-03-26 13:43:33'),
(124, '172ad364-ec92-423a-b042-de7cb3a0d04d', 5080, 'MMC0000594', '068355', NULL, 6229, '832N37330', '2026-03-26', 'RECV', NULL, 1, 'Dalco Metals, Inc.', 'Coil, 10Ga X 17.820', 0, 1, 2, '2026-03-26 13:43:33'),
(125, '0006fc54-1de5-4f04-b4aa-e902ef496738', 5460, 'MMC0000594', '068355', NULL, 6229, '336111', '2026-03-26', 'RECV', NULL, 2, 'Dalco Metals, Inc.', 'Coil, 10Ga X 17.820', 0, 2, 2, '2026-03-26 13:43:33'),
(126, '7fce2ae9-c573-4733-8e86-dd2e3ab77f0a', 5370, 'MMC0000782', '068577', NULL, 6229, '336111', '2026-03-26', 'RECV', NULL, 1, 'Dalco Metals, Inc.', 'Coil, 10Ga X 5.900', 0, 1, 1, '2026-03-26 13:43:33'),
(127, '9ce5c7f9-d4bf-458a-8e55-cb7b42873370', 7280, 'MMC0000777', '068601', NULL, 6229, '335886', '2026-03-26', 'RECV', NULL, 1, 'Dalco Metals, Inc.', 'Coil, 7Ga X 12.375', 0, 1, 2, '2026-03-26 13:43:33'),
(128, '8afb3a38-01ff-4f21-974c-9364e17cd075', 7280, 'MMC0000777', '068601', NULL, 6229, '335886', '2026-03-26', 'RECV', NULL, 2, 'Dalco Metals, Inc.', 'Coil, 7Ga X 12.375', 0, 2, 2, '2026-03-26 13:43:33'),
(129, 'ab567359-da67-4065-868a-71906bfdce7a', 1580, 'MMC0000678', '068601', NULL, 6229, '333250', '2026-03-26', 'RECV', NULL, 1, 'Dalco Metals, Inc.', 'Coil, 11Ga X 5.400', 0, 1, 1, '2026-03-26 13:43:33'),
(130, '8af8bde4-5b69-48af-9fda-7dcac2865347', 2030, 'MMC0000674', '068601', NULL, 6229, '330237', '2026-03-26', 'RECV', NULL, 1, 'Dalco Metals, Inc.', 'Coil, 11Ga X 7.875', 0, 1, 1, '2026-03-26 13:43:33'),
(131, '352a501f-c340-4c48-8c8a-f2a4f85c7830', 4330, 'MMC0000172', '068629', NULL, 6229, '599135', '2026-03-26', 'RECV', NULL, 1, 'Dalco Metals, Inc.', 'Coil, 22Ga X 14.130', 0, 1, 1, '2026-03-26 13:43:33'),
(133, 'd77f4f0f-fe8d-400c-8d32-fc9bc6a4eae0', 1585, 'MMF0005514', '068601', NULL, 6229, '332389', '2026-03-26', 'S-00', NULL, 1, 'Dalco Metals, Inc.', 'Sheet, 14Ga (.075) X 60.000 X 120.000', 0, 1, 1, '2026-03-26 16:39:39'),
(134, 'e87baf80-a504-4a79-943e-b2bd15f10d07', 1880, 'MMC0000427', '068557', NULL, 6229, '568953', '2026-03-26', 'RECV', NULL, 1, 'Dalco Metals, Inc.', 'Coil, 16Ga X 9.250', 0, 1, 1, '2026-03-26 16:39:39'),
(135, 'cac8538f-2e93-4f17-abac-0fec2e3d11db', 3036, 'MMF0005510', '068612', NULL, 6229, '337189', '2026-03-26', 'S-00', NULL, 1, 'Dalco Metals, Inc.', 'Sheet, 10Ga (.134) X 60.000 X 120.000', 0, 1, 1, '2026-03-26 16:39:39'),
(136, 'd810ae31-fb8f-486e-949a-d416edbf7360', 2400, 'MMC0000733', '068629', NULL, 6229, '335428', '2026-03-26', 'RECV', NULL, 1, 'Dalco Metals, Inc.', 'Coil, .187 X 11.400', 0, 1, 1, '2026-03-26 16:39:39'),
(137, 'dba3e255-b361-4a82-8127-d202fc50473c', 6700, 'MMC0000655', '068504', NULL, 6229, '669511', '2026-03-26', 'RECV', NULL, 1, 'Dalco Metals, Inc.', 'Coil, 18Ga X 30.880', 0, 1, 2, '2026-03-26 16:39:39'),
(138, 'fed0a144-82e2-4a1f-ad59-ebbe1a66d888', 6870, 'MMC0000655', '068504', NULL, 6229, '669511', '2026-03-26', 'RECV', NULL, 2, 'Dalco Metals, Inc.', 'Coil, 18Ga X 30.880', 0, 2, 2, '2026-03-26 16:39:39'),
(139, '9768d70f-f4f1-4f24-b2c6-c620253a1cc2', 2968, 'MMF0005507', '068676', NULL, 6229, '335606', '2026-03-26', 'RECV', NULL, 1, 'Dalco Metals, Inc.', 'Sheet, 7Ga (.179) X 60.000 X 120.000', 0, 1, 2, '2026-03-26 16:39:39'),
(140, 'c4db6b53-c6e1-4768-8b72-ed8f1e75c123', 2968, 'MMF0005507', '068676', NULL, 6229, '335606', '2026-03-26', 'RECV', NULL, 2, 'Dalco Metals, Inc.', 'Sheet, 7Ga (.179) X 60.000 X 120.000', 0, 2, 2, '2026-03-26 16:39:39'),
(141, '2d6b2258-e9d8-4255-9888-44b285410b46', 1700, 'MMC0000092', '068579', NULL, 6229, '327309', '2026-03-26', 'RECV', NULL, 1, 'Dalco Metals, Inc.', 'Coil, 10Ga X 1.575', 0, 1, 1, '2026-03-26 16:39:39'),
(142, 'bdc6d441-2a36-4635-a962-44f37178721d', 1525, 'MMF0009014', '068703', NULL, 6229, '842Z38490', '2026-03-26', 'S-00', NULL, 1, 'Dalco Metals, Inc.', 'Sheet, 14Ga (.074) X 60.000 X 120.000', 0, 1, 1, '2026-03-26 16:39:39'),
(143, 'a4a46059-80ed-4c13-bcf3-9149d6e3e557', 3120, 'MMC0000597', '068612', NULL, 6229, '334723', '2026-03-26', 'RECV', NULL, 1, 'Dalco Metals, Inc.', 'Coil, 10Ga X 6.125', 0, 1, 1, '2026-03-26 16:39:39'),
(144, '2cfec9a1-15e9-4bef-a6a2-f38d3fd38cdf', 968, 'MMF0005511', '068755', NULL, 6229, '335598', '2026-03-26', 'S-00', NULL, 1, 'Dalco Metals, Inc.', 'Sheet, 11Ga (.119) X 60.000 X 120.000', 0, 1, 1, '2026-03-26 16:39:39'),
(145, '2e1fa2ba-83be-4724-a53c-db2bb3e3ff62', 4467, 'MMF0005531', '068421', NULL, 6229, 'E5542', '2026-03-26', 'S-00', NULL, 1, 'Dalco Metals, Inc.', 'Sheet, (.312) X 60.000 X 120.000', 0, 1, 10, '2026-03-26 16:39:39'),
(146, '785cb84a-e939-4517-8c3d-1376d6b6f859', 4467, 'MMF0005531', '068421', NULL, 6229, 'E5542', '2026-03-26', 'S-00', NULL, 2, 'Dalco Metals, Inc.', 'Sheet, (.312) X 60.000 X 120.000', 0, 2, 10, '2026-03-26 16:39:39'),
(147, 'dda7eeca-2c09-46ac-b2ae-6dc12aa1a3e5', 4467, 'MMF0005531', '068421', NULL, 6229, 'E5542', '2026-03-26', 'S-00', NULL, 3, 'Dalco Metals, Inc.', 'Sheet, (.312) X 60.000 X 120.000', 0, 3, 10, '2026-03-26 16:39:39'),
(148, '6110bcc1-53a6-4196-9a52-1a35e860d2c8', 4467, 'MMF0005531', '068421', NULL, 6229, 'E5542', '2026-03-26', 'S-00', NULL, 4, 'Dalco Metals, Inc.', 'Sheet, (.312) X 60.000 X 120.000', 0, 4, 10, '2026-03-26 16:39:39'),
(149, 'a6feecea-ffc1-4679-8178-3030c485575d', 4467, 'MMF0005531', '068421', NULL, 6229, 'E5542', '2026-03-26', 'S-00', NULL, 5, 'Dalco Metals, Inc.', 'Sheet, (.312) X 60.000 X 120.000', 0, 5, 10, '2026-03-26 16:39:39'),
(150, 'd2e446ce-35a4-4bfb-97f1-29fbd7557b88', 4467, 'MMF0005531', '068421', NULL, 6229, 'E5542', '2026-03-26', 'S-00', NULL, 6, 'Dalco Metals, Inc.', 'Sheet, (.312) X 60.000 X 120.000', 0, 6, 10, '2026-03-26 16:39:39'),
(151, 'c3cae23a-7f58-47e0-a6bf-82b12476b38b', 4467, 'MMF0005531', '068421', NULL, 6229, 'E5542', '2026-03-26', 'S-00', NULL, 7, 'Dalco Metals, Inc.', 'Sheet, (.312) X 60.000 X 120.000', 0, 7, 10, '2026-03-26 16:39:39'),
(152, 'ec535431-afae-49df-8f84-2eddb11f6061', 4467, 'MMF0005531', '068421', NULL, 6229, 'E5542', '2026-03-26', 'S-00', NULL, 8, 'Dalco Metals, Inc.', 'Sheet, (.312) X 60.000 X 120.000', 0, 8, 10, '2026-03-26 16:39:39'),
(153, 'bbafdba5-1df7-4566-9197-b731a73d197b', 5110, 'MMF0005531', '068421', NULL, 6229, 'E5542', '2026-03-26', 'S-00', NULL, 9, 'Dalco Metals, Inc.', 'Sheet, (.312) X 60.000 X 120.000', 0, 9, 10, '2026-03-26 16:39:39'),
(154, '20e8ada2-97ca-48d4-852d-a149a581802a', 5110, 'MMF0005531', '068421', NULL, 6229, 'E5542', '2026-03-26', 'S-00', NULL, 10, 'Dalco Metals, Inc.', 'Sheet, (.312) X 60.000 X 120.000', 0, 10, 10, '2026-03-26 16:39:39'),
(164, '83f36f2e-b910-434d-ae51-e310b755995e', 4062, 'MMC0000014', '068597', NULL, 6229, 'Nothing Entered', '2026-03-26', 'RECV', NULL, 1, 'Mead Metals, Inc', 'Coil, 14Ga X 2.250', 0, 1, 2, '2026-03-26 17:25:37'),
(165, 'a8303d34-e6ac-448f-bff6-42c72d5cdaa6', 1978, 'MMC0000014', '068597', NULL, 6229, 'Nothing Entered', '2026-03-26', 'RECV', NULL, 2, 'Mead Metals, Inc', 'Coil, 14Ga X 2.250', 0, 2, 2, '2026-03-26 17:25:37');

-- NOTE: Primary key, indexes, and AUTO_INCREMENT are defined in
-- 10_Table_receiving_history.sql (CREATE TABLE). Do not redefine them here.
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;

