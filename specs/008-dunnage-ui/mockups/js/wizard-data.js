// Sample Data for Mockup
const dunnageTypes = [
    'Pallet',
    'Cardboard Box',
    'Wooden Crate',
    'Plastic Container',
    'Metal Bin',
    'Bubble Wrap',
    'Foam Padding',
    'Stretch Wrap',
    'Corner Protectors',
    'Edge Guards',
    'Anti-Static Bags'
];

const partsByType = {
    'Pallet': [
        { id: 'PALLET-48X40', width: 48, height: 40, depth: 6, inventoried: true },
        { id: 'PALLET-48X48', width: 48, height: 48, depth: 6, inventoried: true },
        { id: 'PALLET-40X48', width: 40, height: 48, depth: 6, inventoried: true },
        { id: 'PALLET-EURO', width: 47.24, height: 31.50, depth: 6, inventoried: false }
    ],
    'Cardboard Box': [
        { id: 'BOX-SMALL', width: 12, height: 12, depth: 8, inventoried: false },
        { id: 'BOX-MEDIUM', width: 18, height: 18, depth: 12, inventoried: false },
        { id: 'BOX-LARGE', width: 24, height: 24, depth: 18, inventoried: false }
    ],
    'Wooden Crate': [
        { id: 'CRATE-24X24', width: 24, height: 24, depth: 24, inventoried: true },
        { id: 'CRATE-36X36', width: 36, height: 36, depth: 36, inventoried: true }
    ],
    'Plastic Container': [
        { id: 'TOTE-BLUE', width: 20, height: 15, depth: 12, inventoried: false },
        { id: 'TOTE-RED', width: 20, height: 15, depth: 12, inventoried: false }
    ],
    'Metal Bin': [
        { id: 'BIN-STEEL-LG', width: 30, height: 20, depth: 15, inventoried: true }
    ],
    'Bubble Wrap': [
        { id: 'BUBBLE-12IN', width: 12, height: 0, depth: 0, inventoried: false },
        { id: 'BUBBLE-24IN', width: 24, height: 0, depth: 0, inventoried: false }
    ],
    'Foam Padding': [
        { id: 'FOAM-1IN', width: 48, height: 48, depth: 1, inventoried: false },
        { id: 'FOAM-2IN', width: 48, height: 48, depth: 2, inventoried: false }
    ],
    'Stretch Wrap': [
        { id: 'WRAP-18IN', width: 18, height: 0, depth: 0, inventoried: false }
    ],
    'Corner Protectors': [
        { id: 'CORNER-3IN', width: 3, height: 3, depth: 48, inventoried: false }
    ],
    'Edge Guards': [
        { id: 'EDGE-2IN', width: 2, height: 48, depth: 0, inventoried: false }
    ],
    'Anti-Static Bags': [
        { id: 'STATIC-SM', width: 6, height: 9, depth: 0, inventoried: false },
        { id: 'STATIC-LG', width: 12, height: 18, depth: 0, inventoried: false }
    ]
};

const typeSpecs = {
    'Pallet': [
        { name: 'Width', type: 'number', unit: 'inches', required: true },
        { name: 'Height', type: 'number', unit: 'inches', required: true },
        { name: 'Depth', type: 'number', unit: 'inches', required: true },
        { name: 'IsInventoriedToVisual', type: 'boolean', required: false }
    ],
    'Cardboard Box': [
        { name: 'Width', type: 'number', unit: 'inches', required: true },
        { name: 'Height', type: 'number', unit: 'inches', required: true },
        { name: 'Depth', type: 'number', unit: 'inches', required: true },
        { name: 'Material', type: 'text', required: false }
    ],
    'Wooden Crate': [
        { name: 'Width', type: 'number', unit: 'inches', required: true },
        { name: 'Height', type: 'number', unit: 'inches', required: true },
        { name: 'Depth', type: 'number', unit: 'inches', required: true },
        { name: 'WoodType', type: 'text', required: false }
    ],
    'Plastic Container': [
        { name: 'Width', type: 'number', unit: 'inches', required: true },
        { name: 'Height', type: 'number', unit: 'inches', required: true },
        { name: 'Depth', type: 'number', unit: 'inches', required: true },
        { name: 'Color', type: 'text', required: false }
    ],
    'Metal Bin': [
        { name: 'Width', type: 'number', unit: 'inches', required: true },
        { name: 'Height', type: 'number', unit: 'inches', required: true },
        { name: 'Depth', type: 'number', unit: 'inches', required: true },
        { name: 'Material', type: 'text', required: false }
    ],
    'Bubble Wrap': [
        { name: 'Width', type: 'number', unit: 'inches', required: true },
        { name: 'BubbleSize', type: 'text', required: false }
    ],
    'Foam Padding': [
        { name: 'Width', type: 'number', unit: 'inches', required: true },
        { name: 'Height', type: 'number', unit: 'inches', required: true },
        { name: 'Thickness', type: 'number', unit: 'inches', required: true }
    ],
    'Stretch Wrap': [
        { name: 'Width', type: 'number', unit: 'inches', required: true },
        { name: 'Thickness', type: 'number', unit: 'mil', required: false }
    ],
    'Corner Protectors': [
        { name: 'Length', type: 'number', unit: 'inches', required: true },
        { name: 'Thickness', type: 'number', unit: 'inches', required: false }
    ],
    'Edge Guards': [
        { name: 'Length', type: 'number', unit: 'inches', required: true },
        { name: 'Width', type: 'number', unit: 'inches', required: false }
    ],
    'Anti-Static Bags': [
        { name: 'Width', type: 'number', unit: 'inches', required: true },
        { name: 'Height', type: 'number', unit: 'inches', required: true },
        { name: 'Thickness', type: 'number', unit: 'mil', required: false }
    ]
};
