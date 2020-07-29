import { DataTable } from "./dataTable";
import { MobileDataClient } from './mobileDataClient';
import { BlogPost, BlogComment } from "./models";

export class BlogPostMobileDataTable extends DataTable<BlogPost> {
    constructor(client: MobileDataClient, tableName: string) {
        super(client, tableName);
    }

    create(bodyAsText: string): BlogPost {
        return {
            title: "blog-post-title",
            commentCount: 0,
            showComments: false,
            data: bodyAsText,
            id: "foo",
            updatedAt: undefined,
            version: undefined,
            deleted: false,
        };
    }
}

export class BlogCommentMobileDataTable extends DataTable<BlogComment> {
    constructor(client: MobileDataClient, tableName: string) {
        super(client, tableName);
    }

    create(bodyAsText: string): BlogComment {
        return {
            postId: "foo-id",
            commentText: bodyAsText,
            name: "post-" + bodyAsText,
            test: 0
        }
    }
}
