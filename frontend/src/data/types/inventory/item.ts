interface BaseItemFields {
	id: number;
	name: string;
	description: string | null;
	imageUrl: string | null;
	price: number;
	totalQuantity: number;
	alertQuantity: number;
	isLowStock: boolean;
	lastUpdate: string;
	isBook: boolean;
	isActive: boolean;
}

export interface ItemApiDTO extends BaseItemFields {
	distributor?: string | null;
	isbn?: string | null;
	publicationDate?: string | null;
	authors?: string[];
	authorIds?: number[];
	publishers?: string[];
	categories?: string[];
	categoryIds?: number[];
}

export interface Item extends BaseItemFields {
	distributor: string | null;
	isbn: string | null;
	publicationDate: string | null;
	authors: string[];
	authorIds?: number[];
	publishers: string[];
	categories: string[];
	categoryIds: number[];
}

export interface CreateItemRequest {
	name: string;
	description: string | null;
	distributor?: string | null;
	price: number;
	alertQuantity: number;
	imageUrl?: null;
}

export interface CreateBookRequest extends CreateItemRequest {
	isbn: string;
	publicationDate: string;
	authors?: string[];
	publishers?: string[];
	authorIds: number[];
	categoryIds: number[];
	publisherIds: number[];
}
