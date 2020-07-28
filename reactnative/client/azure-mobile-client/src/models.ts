export interface EntityTableData {
    id?: string,
    updatedAt?: Date,
    version?: string,
    deleted?: boolean,
}

export interface BlogComment extends EntityTableData {
    postId: string,
    commentText: string,
    name: string,
    test: number,
}

export interface BlogPost extends EntityTableData {
    title: string,
    commentCount: number,
    showComments: boolean,
    data: string,
}
