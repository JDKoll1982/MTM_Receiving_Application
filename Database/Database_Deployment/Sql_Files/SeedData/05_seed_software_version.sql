INSERT INTO software_version (
    id,
    required_version,
    updated_by
) VALUES (
    1,
    '2.5.0',
    'seed'
)
ON DUPLICATE KEY UPDATE
    required_version = VALUES(required_version),
    updated_by = VALUES(updated_by),
    updated_at = NOW();