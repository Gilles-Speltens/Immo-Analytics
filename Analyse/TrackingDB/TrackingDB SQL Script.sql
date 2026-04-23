CREATE TABLE `site` (
  `domain` varchar(255) PRIMARY KEY,
  `date_when_added` timestamp NOT NULL,
  `certify` bool DEFAULT false
);

CREATE TABLE `sessions` (
  `id` bigint PRIMARY KEY,
  `session_id` varchar(36),
  `site` varchar(255) NOT NULL,
  `user_id` varchar(36),
  `user_ip` varchar(45) NOT NULL,
  `language_browser` varchar(255) NOT NULL,
  `user_agent` varchar(255) NOT NULL,
  `session_start` timestamp NOT NULL,
  `session_end` timestamp
);

CREATE TABLE `hit_page` (
  `id` bigint PRIMARY KEY,
  `time` timestamp NOT NULL,
  `session_pk` bigint NOT NULL,
  `url` text NOT NULL,
  `referrer` text
);

CREATE TABLE `user_actions` (
  `id` bigint PRIMARY KEY AUTO_INCREMENT,
  `time` timestamp NOT NULL,
  `page_id` bigint NOT NULL,
  `type` enum('UNKNOWN','HITPAGE', 'ESTATE_SEARCH','CONTACT_REQUEST', 'CLIENT_ACTION') NOT NULL,
  `parameter` json
);

CREATE TABLE `file_monitoring` (
  `file_name` char(25) PRIMARY KEY,
  `treated_date` timestamp not null,
  `speed` int,
  `treated_logs` int DEFAULT 0,
  `skipped_logs` int DEFAULT 0,
  `status` enum('TREATED', 'IN_PROCESS', 'FAILED') NOT NULL
);


CREATE UNIQUE INDEX `sessions_index_session_site` ON `sessions` (`session_id`, `site`);

ALTER TABLE `user_actions` ADD FOREIGN KEY (`page_id`) REFERENCES `hit_page` (`id`);

ALTER TABLE `hit_page` ADD FOREIGN KEY (`session_pk`) REFERENCES `sessions` (`id`);

ALTER TABLE `sessions` ADD FOREIGN KEY (`site`) REFERENCES `site` (`domain`);
ALTER TABLE `sessions` ADD CONSTRAINT `unique_session_site_constraint` UNIQUE (`session_id`, `site`);

ALTER TABLE `site` ADD CONSTRAINT `unique_domain_constraint` UNIQUE (`domain`);